//using system.collections;
//using system.collections.generic;
//using unityengine;
//using unityengine.ui;

//[system.serializable]
//public class node
//{
//    /// <summary>
//    /// 노드 생성자
//    /// </summary>
//    /// <param name="_iswall"> 현재 벽인지 판별 </param>
//    /// <param name="x"> 노드의 x 포지션 </param>
//    /// <param name="y"> 노드의 y 포지션 </param>
//    public node(bool _iswall, int x, int y)
//    {
//        iswall = _iswall;

//        nodepos.x = x;
//        nodepos.y = y;
//    }

//    [tooltip("현재 노드가 벽인지 판별")]
//    public bool iswall;

//    [tooltip("현재 노드의 부모 노드(이전 노드)")]
//    public node parentnode;

//    [tooltip("현재 노드의 포지션")]
//    public vector2int nodepos;

//    [tooltip("시작으로부터 이동한 거리")]
//    public int gcost;

//    [tooltip("장애물을 무시한 목표까지의 거리 (가로, 세로)")]
//    public int hcost;

//    public int fcost //g + h
//    {
//        get
//        {
//            return gcost + hcost;
//        }
//    }
//}
//public class basicenemy : monobehaviour
//{
//    [serializefield]
//    [tooltip("기본 적 정보(scriptableobject)")]
//    protected enemydata enemydata;

//    [tooltip("체력")]
//    private int hp;

//    [serializefield]
//    [tooltip("체력바 이미지 컴포넌트")]
//    protected image hpbarimg;

//    [serializefield]
//    [tooltip("spriterenderer 컴포넌트")]
//    protected spriterenderer sr;

//    [serializefield]
//    [tooltip("현재 감지한 플레이어의 player 컴포넌트")]
//    protected player playercomponent;

//    [tooltip("코루틴 딜레이 : 0.1초")]
//    private waitforseconds zerotoone = new waitforseconds(0.1f);

//    [tooltip("현재 실행중인 타격 효과 코루틴")]
//    private ienumerator hiteffectcoroutine;

//    #region 이동 관련 요소들 모음
//    [header("이동 관련 요소들 모음")]

//    const int move_straight_cost = 10;
//    const int move_diagonal_cost = 14;

//    [tooltip("현재 맵 크기 저장용 vector")]
//    private vector2int mapsize;

//    [tooltip("현재 맵 기준 타일 위치 vector (맨 왼쪽 아래 타일)")]
//    private vector2int criteriatilepos;

//    //[tooltip("추격 후 플레이어 감지 범위")]
//    //private const int updatedetectedrange = 50;

//    [tooltip("현재 노드가 벽인지 판별")]
//    private bool iswall;

//    [tooltip("경로 탐색 시 시작 포지션")]
//    private vector3 startpos;

//    [tooltip("경로 탐색 시 목표 포지션")]
//    private vector3 targetpos;

//    [tooltip("최종 경로 노드 리스트")]
//    public list<node> finalnodelist;

//    //[tooltip("전체 경로 범위 노드 배열")]
//    //private node[,] nodearray = new node[updatedetectedrange * 2 + 1, updatedetectedrange * 2 + 1];

//    [tooltip("경로 시작 노드")]
//    private node startnode;

//    [tooltip("목적지 노드")]
//    private node targetnode;

//    //[tooltip("현재 경로 노드")]
//    //private node curnode;

//    [tooltip("경로 탐색 가능한 노드 리스트")]
//    private list<node> openlist = new list<node>();

//    private list<node> mapdata = new list<node>();

//    //[tooltip("경로 탐색 완료한 노드 리스트")]
//    //private list<node> closedlist = new list<node>();

//    [tooltip("벽 태그")]
//    private const string wall = "wall";
//    #endregion

//    private void awake()
//    {
//        startsetting();
//    }

//    /// <summary>
//    /// 시작 세팅 함수
//    /// </summary>
//    private void startsetting()
//    {
//        hp = enemydata.maxhp;

//        mapsize = new vector2int(87, 66); //게임 매니저에서 현재 스테이지에 맞는 맵 크기 받아오는 것으로 수정
//        criteriatilepos = new vector2int(-35, -37);
//    }

//    /// <summary>
//    /// 피격 함수
//    /// </summary>
//    /// <param name="damage"> 받은 데미지 </param>
//    public void hit(int damage)
//    {
//        hp -= damage;

//        hpbarimg.fillamount = (float)hp / enemydata.maxhp;

//        if (hp <= 0)
//        {
//            dead();
//        }
//        else 
//        {
//            if (hiteffectcoroutine != null)
//            {
//                stopcoroutine(hiteffectcoroutine);    
//            }

//            hiteffectcoroutine = hiteffect();
//            startcoroutine(hiteffectcoroutine);
//        }
//    }

//    /// <summary>
//    /// 타격 효과 함수
//    /// </summary>
//    /// <returns></returns>
//    private ienumerator hiteffect()
//    {
//        sr.color = color.red;

//        yield return zerotoone;

//        sr.color = color.white;
//    }

//    /// <summary>
//    /// 사망 함수
//    /// </summary>
//    private void dead()
//    {
//        stagemanager.instance.plusscore(enemydata.score);

//        destroy(gameobject);
//    }

//    /// <summary>
//    /// 플레이어 감지 시작
//    /// </summary>
//    public virtual void detectedplayer(gameobject detectedplayerobj)
//    {
//        if (playercomponent != null)
//        {
//            return;
//        }

//        playercomponent = detectedplayerobj.getcomponent<player>();

//        startcoroutine(pathfind());
//    }

//    private ienumerator pathfind()
//    {
//        waitforseconds pointzerofivesec = new waitforseconds(0.05f);

//        #region  초가화 

//        openlist.clear();
//        mapdata.clear();
//        mapsetting();

//        #endregion

//        startnode.gcost = 0;
//        startnode.hcost = heuristic(startnode.position, endnode.position);
//        openlist.add(startnode);

//        // pathfinding
//        while (opennodes.count > 0)
//        {
//            node currentnode = getlowestfcost(opennodes);
//            opennodes.remove(currentnode);


//            #region  시각화 하기 위한 곳 오픈 리스트에 들어간 노드를 초록으로 변경
//            if (currentnode != startnode)
//            {
//                currentnode.tile.setcolor(color.green);
//            }
//            #endregion

//            if (currentnode == endnode) // 현재 노드가 끝 노드라면
//            {
//                print("find");
//                showpath(endnode);
//                yield break;
//            }

//            if (currentnode.moveable == true)
//            {
//                right(currentnode);
//                yield return pointzerofivesec;

//                down(currentnode);
//                yield return pointzerofivesec;

//                left(currentnode);
//                yield return pointzerofivesec;

//                up(currentnode);
//                yield return pointzerofivesec;

//                rightup(currentnode);
//                yield return pointzerofivesec;

//                rightdown(currentnode);
//                yield return pointzerofivesec;

//                leftup(currentnode);
//                yield return pointzerofivesec;

//                leftdown(currentnode);
//                yield return pointzerofivesec;
//            }

//            currentnode.moveable = false;    // 한번 간 노드 

//            yield return pointzerofivesec;
//        }
//    }

//    #region  오른쪽 검사

//    private bool right(node _startnode, bool ismovealbe = false)
//    {
//        int x = _startnode.position.x;
//        int y = _startnode.position.y;

//        node startnode = _startnode;
//        node currentnode = startnode;

//        bool isfind = false;

//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = ismovealbe;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x + 1, y) == false)   // 탐색 가능 여부
//            {
//                break;
//            }
//            currentnode = getgrid(++x, y); // 다음 오른쪽 노드로 이동

//            if (currentnode.moveable == false)
//            {
//                break;
//            }

//            if (currentnode == this.endnode)
//            {
//                isfind = true;
//                if (ismovealbe == false)
//                {
//                    addopenlist(currentnode, startnode);
//                }
//                break;
//            }

//            currentnode.tile.setcolor(color.gray);

//            #endregion

//            #region 위쪽이 막혀 있으면서 오른쪽 위는 뚫려있는 경우

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y + 1).moveable == false) // 위쪽이 벽이면
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // 오른쪽 위가 막혀있지 않으면
//                    {
//                        if (ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        isfind = true;
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 오른쪽 아래는 뚫려있는 경우

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y - 1).moveable == false) // 아래쪽이 벽이고
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) // 오른쪽 아래가 막혀 있지 않으면
//                    {
//                        if (ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        isfind = true;
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        startnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  왼쪽 검사

//    private bool left(node _startnode, bool _ismovealbe = false)
//    {
//        int x = _startnode.position.x;
//        int y = _startnode.position.y;

//        node startnode = _startnode;
//        node currentnode = startnode;

//        bool isfind = false;

//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = _ismovealbe;


//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x - 1, y) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentnode = getgrid(--x, y); // 다음 왼쪽 노드로 이동
//            if (currentnode.moveable == false)
//            {
//                break;
//            }
//            currentnode.tile.setcolor(color.gray);

//            if (currentnode == this.endnode)
//            {
//                isfind = true;

//                if (_ismovealbe == false)
//                {
//                    addopenlist(currentnode, startnode);
//                }
//                break;
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우

//            if (y + 1 < mapsize.y && x - 1 >= 0)
//            {
//                if (getgrid(x, y + 1).moveable == false) // 위쪽이 벽이면
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 막혀있지 않으면
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우

//            if (y > 0 && x - 1 >= 0)
//            {
//                if (getgrid(x, y - 1).moveable == false) // 아래쪽이 벽이고
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // 왼쪽 아래가 막혀 있지 않으면
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        startnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  위쪽 검사

//    private bool up(node _satrtnode, bool _ismovealbe = false)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;

//        bool isfind = false;

//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = _ismovealbe;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x, y + 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentnode = getgrid(x, ++y); // 다음 왼쪽 노드로 이동
//            if (currentnode.moveable == false)
//            {
//                break;
//            }
//            currentnode.tile.setcolor(color.gray);

//            if (currentnode == this.endnode)
//            {
//                isfind = true;
//                if (_ismovealbe == false)
//                {
//                    addopenlist(currentnode, startnode);
//                }

//                break;
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // 오른쪽이 벽이면
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // 오른쪽 위가
//                    {
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        isfind = true;
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우

//            if (y + 1 < mapsize.y && x > 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // 왼쪽이 벽이고
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 막혀 있지 않으면
//                    {
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        isfind = true;
//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        _satrtnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  아래쪽 검사

//    private bool down(node _satrtnode, bool _ismovealbe = false)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;

//        bool isfind = false;

//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = _ismovealbe;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x, y - 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentnode = getgrid(x, --y); // 다음 왼쪽 노드로 이동
//            if (currentnode.moveable == false)
//            {
//                break;
//            }
//            currentnode.tile.setcolor(color.gray);

//            if (currentnode == this.endnode)
//            {
//                isfind = true;
//                if (_ismovealbe == false)
//                {
//                    addopenlist(currentnode, startnode);
//                }

//                break;
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 위는 뚫려있는 경우

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // 오른쪽이 벽이면
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) // 오른쪽 아래가 갈 수 있다면
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀 있으면서 왼쪽 아래는 뚫려있는 경우

//            if (y > 0 && x - 1 >= 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // 왼쪽이 벽이고
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // 왼쪽 아래가 막혀 있지 않으면
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion
//        }

//        _satrtnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  오른쪽 위 대각선 검사

//    private void rightup(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;


//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x + 1, y + 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }



//            currentnode = getgrid(++x, ++y); // 다음 왼쪽 노드로 이동

//            if (currentnode.moveable == false)
//            {
//                break;
//            }

//            if (currentnode == this.endnode)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            currentnode.tile.setcolor(color.gray);

//            #endregion

//            #region 왼쪽이 막혀 있으면서 왼쪽 위가 막혀있지 않은 경우

//            if (y + 1 < mapsize.y && x > 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // 왼쪽이 막힘
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 안막힘
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 아래가 막혀 있으면서 오른쪽 아래가 안막혔으면

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y - 1).moveable == false) // 왼쪽이 벽이고
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) // 왼쪽 아래가 막혀 있지 않으면
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (right(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            if (up(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }
//        }

//        _satrtnode.moveable = true;
//    }

//    #endregion


//    #region  오른쪽 아래 대각선 검사

//    private void rightdown(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;

//        while (currentnode.moveable == true)
//        {

//            currentnode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x + 1, y - 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentnode = getgrid(++x, --y); // 다음 왼쪽 노드로 이동

//            if (currentnode.moveable == false)
//            {
//                break;
//            }

//            if (currentnode == this.endnode)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            currentnode.tile.setcolor(color.gray);

//            #endregion

//            #region 왼쪽이 막혀있고 왼쪽 아래가 막혀 있지 않으면

//            if (y > 0 && x > 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // 왼쪽이 막힘
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // 왼쪽 위가 안막힘
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 위쪽이 막혀있고 오른쪽 위가 막혀있지 않으면

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y + 1).moveable == false) // 위쪽이 벽이고
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // 오른쪽 위가 막혀있지 않으면
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (right(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            if (down(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }
//        }

//        _satrtnode.moveable = true;
//    }

//    #endregion


//    #region  왼쪽 위 대각선 검사

//    private void leftup(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;


//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x - 1, y + 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentnode = getgrid(--x, ++y); // 다음 왼쪽 노드로 이동

//            if (currentnode.moveable == false)
//            {
//                break;
//            }

//            if (currentnode == this.endnode)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            currentnode.tile.setcolor(color.gray);

//            #endregion

//            #region 오른쪽이 막혀있고 오른쪽 위가 막혀있지 않으면

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // 오른쪽이 막힘
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // 오른쪽 위가 안막힘
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 아래가 막혀있고 왼쪽 아래가 막혀있지 않으면

//            if (y > 0 && x > 0)
//            {
//                if (getgrid(x, y - 1).moveable == false) // 아래가 벽
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // 왼쪽 아래가 안막힘
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (left(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            if (up(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }
//        }

//        _satrtnode.moveable = true;
//    }

//    #endregion

//    #region  왼쪽 아래 대각선 검사

//    private void leftdown(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;


//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = false;

//            #region 맵 사이즈가 넘어가는지 안 넘어가지는지를 체크

//            if (comp(x - 1, y - 1) == false)   // 탐색 가능 여부
//            {
//                break;
//            }

//            currentnode = getgrid(--x, --y); // 다음 왼쪽 노드로 이동

//            if (currentnode.moveable == false)
//            {
//                break;
//            }

//            if (currentnode == this.endnode)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            currentnode.tile.setcolor(color.gray);

//            #endregion

//            #region 오른쪽이 막혀있고 오른쪽 위가 막혀있지 않으면

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // 오른쪽이 막힘
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) //오른쪽 아래가 안 막힘
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }

//            #endregion

//            #region 아래가 막혀있고 왼쪽 아래가 막혀있지 않으면

//            if (y + 1 < mapsize.y && x < 0)
//            {
//                if (getgrid(x, y + 1).moveable == false) // 위가 막힘
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // 왼쪽 위가 안막힘
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // 코너 발견하면 바로 종료
//                    }
//                }
//            }


//            #endregion

//            if (left(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }

//            if (down(currentnode, true) == true)
//            {
//                addopenlist(currentnode, startnode);

//                break;
//            }
//        }

//        _satrtnode.moveable = true;
//    }

//    #endregion

//    #region  탐색 가능 여부
//    private bool comp(int _x, int _y)
//    {
//        if (_x < 0 || _y < 0 || _x >= mapsize.x || _y >= mapsize.y)
//        {
//            return false;
//        }
//        return true;
//    }
//    #endregion


//    #region 리스트 안에 있는 노드를 x와 y좌표 값으로 반환

//    private node getgrid(int _x, int _y)
//    {
//        return mapdata[_x + _y * mapsize.x];
//    }

//    #endregion


//    /// <summary>
//    /// 맵 세팅 함수
//    /// </summary>
//    private void mapsetting()
//    {
//        int x; //
//        int y;

//        node newnode;

//        for (int i = 0; i < mapsize.y; i++)
//        {
//            for (int j = 0; j < mapsize.x; j++)
//            {
//                foreach (collider2d collider in physics2d.overlapcircleall(new vector2(startpos.x + i, startpos.y + j), 0.2f))
//                {
//                    if (collider.gameobject.comparetag(wall))
//                    {
//                        iswall = true;
//                    }
//                }

//                newnode = new node(iswall, criteriatilepos.x + i, criteriatilepos.y);
//                mapdata.add(newnode);
//            }

//            // add wall
//            if (child.comparetag("start"))
//            {
//                startnode = newnode;
//                newnode.tile.setcolor(color.red);
//            }
//            else if (child.comparetag("end"))
//            {
//                endnode = newnode;
//                newnode.tile.setcolor(color.blue);
//            }
//            else if (child.comparetag("wall"))
//            {
//                newnode.tile.setcolor(color.black);
//            }
//            else
//            {
//                newnode.tile.setcolor(color.white);
//            }
//        }
//    }
//    #endregion


//    #region 도착 노드까지 가장 짧은 경로의 값을 반환 휴리스틱
//    private int heuristic(vector2int _currposition, vector2int _endposition)
//    {
//        int x = mathf.abs(_currposition.x - _endposition.x);
//        int y = mathf.abs(_currposition.y - _endposition.y);
//        int reming = mathf.abs(x - y);

//        return move_diagonal_cost * mathf.min(x, y) + move_straight_cost * reming;
//    }

//    #endregion


//    #region  리스트 중에 가장 짧은 f값을 가진 노드를 반환
//    private node getlowestfcost(list<node> _pathlist)
//    {
//        node lowestnode = _pathlist[0];

//        for (int i = 1; i < _pathlist.count; i++)
//        {
//            if (_pathlist[i].fcost < lowestnode.fcost)
//            {
//                lowestnode = _pathlist[i];
//            }
//        }
//        return lowestnode;
//    }


//    #endregion

//    private void showpath(node _node)
//    {
//        if (_node != null)
//        {
//            if (_node == startnode)
//            {
//                _node.tile.setcolor(color.red);
//            }
//            else if (_node == endnode)
//            {
//                _node.tile.setcolor(color.blue);
//            }
//            else
//            {
//                _node.tile.setcolor(color.cyan);
//            }

//            if (_node.parentnode != null)
//            {
//                vector3 start = _node.tile.transform.position;
//                vector3 end = _node.parentnode.tile.transform.position;
//                debug.drawline(start, end, color.yellow, 5);
//            }

//            showpath(_node.parentnode);
//        }
//    }

//    private void addopenlist(node _currentnode, node _parentnode)
//    {
//        int nextcost = _parentnode.gcost + heuristic(_parentnode.position, _currentnode.position);
//        //if (nextcost < _currentnode.gcost)
//        // {
//        _currentnode.parentnode = _parentnode;
//        _currentnode.gcost = _parentnode.gcost + heuristic(_parentnode.position, _currentnode.position);
//        _currentnode.hcost = heuristic(_currentnode.position, endnode.position);
//        opennodes.add(_currentnode);
//        // }
//    }

//    ///// <summary>
//    ///// 경로 탐색 함수
//    ///// </summary>
//    //public void pathfinding()
//    //{
//    //    startpos = transform.position;
//    //    targetpos = playercomponent.movetargetpos;

//    //    for (int i = -updatedetectedrange; i <= updatedetectedrange; i++) //플레이어 추격 범위만큼 노드 세팅
//    //    {
//    //        for (int j = -updatedetectedrange; j <= updatedetectedrange; j++)
//    //        {
//    //            iswall = false;

//    //            foreach (collider2d collider in physics2d.overlapcircleall(new vector2(startpos.x + i, startpos.y + j), 0.2f))
//    //            {
//    //                if (collider.gameobject.comparetag(wall))
//    //                {
//    //                    iswall = true;
//    //                }
//    //            }

//    //            nodearray[i + updatedetectedrange, j + updatedetectedrange] = new node(iswall, (int)startpos.x + i, (int)startpos.y + j);
//    //        }
//    //    }

//    //    startnode = nodearray[updatedetectedrange, updatedetectedrange];

//    //    targetnode = nodearray[(int)targetpos.x - (int)startpos.x + updatedetectedrange, (int)targetpos.y - (int)startpos.y + updatedetectedrange];

//    //    openlist.clear();

//    //    openlist.add(startnode);

//    //    closedlist.clear();

//    //    finalnodelist.clear();

//    //    while (openlist.count > 0)
//    //    {
//    //        // 오픈리스트 중 가장 f가 작은 것, f가 같다면 h가 작은 것, h도 같다면 0번째 것을 현재노드로 하고 열린리스트에서 닫힌리스트로 옮기기
//    //        curnode = openlist[0];

//    //        openlist.remove(curnode);
//    //        closedlist.add(curnode);

//    //        for (int i = 0; i < openlist.count; i++)
//    //        {
//    //            if (openlist[i].fcost <= curnode.fcost && openlist[i].hcost < curnode.hcost)
//    //            {
//    //                curnode = openlist[i];
//    //            }
//    //        }

//    //        // 마지막
//    //        if (curnode == targetnode)
//    //        {
//    //            node targetcurnode = targetnode;

//    //            while (targetcurnode != startnode)
//    //            {
//    //                finalnodelist.add(targetcurnode);
//    //                targetcurnode = targetcurnode.parentnode;
//    //            }

//    //            finalnodelist.add(startnode);
//    //            finalnodelist.reverse();

//    //            startcoroutine(move());

//    //            return;
//    //        }

//    //        openlistadd(curnode.xpos, curnode.ypos + 1);
//    //        openlistadd(curnode.xpos + 1, curnode.ypos);
//    //        openlistadd(curnode.xpos, curnode.ypos - 1);
//    //        openlistadd(curnode.xpos - 1, curnode.ypos);
//    //    }
//    //}

//    ///// <summary>
//    ///// 진행 가능한 경로의 노드들 오픈리스트 추가 함수
//    ///// </summary>
//    ///// <param name="checkx"></param>
//    ///// <param name="checky"></param>
//    //private void openlistadd(int checkx, int checky)
//    //{
//    //    //감지 범위를 벗어나지 않고, 벽이 아니면서, 닫힌리스트에 없다면
//    //    if (checkx >= startpos.x - updatedetectedrange && checkx <= startpos.x + updatedetectedrange
//    //        && checky >= startpos.y - updatedetectedrange && checky <= startpos.y + updatedetectedrange
//    //        && nodearray[(int)(checkx - startpos.x) + updatedetectedrange, (int)(checky - startpos.y) + updatedetectedrange].iswall == false
//    //        && closedlist.contains(nodearray[(int)(checkx - startpos.x) + updatedetectedrange, (int)(checky - startpos.y) + updatedetectedrange]) == false)
//    //    {
//    //        // 이웃노드에 넣기
//    //        node neighbornode = nodearray[(int)(checkx - startpos.x) + updatedetectedrange, (int)(checky - startpos.y) + updatedetectedrange];
//    //        int movecost = 10;

//    //        // 이동비용이 이웃노드 g보다 작거나 또는 열린리스트에 이웃노드가 없다면 g, h, parentnode를 설정 후 열린리스트에 추가
//    //        if (movecost < neighbornode.gcost || openlist.contains(neighbornode) == false)
//    //        {
//    //            neighbornode.gcost = movecost;
//    //            neighbornode.hcost = (mathf.abs(neighbornode.xpos - targetnode.xpos) + mathf.abs(neighbornode.ypos - targetnode.ypos)) * 10;
//    //            neighbornode.parentnode = curnode;

//    //            openlist.add(neighbornode);
//    //        }
//    //    }
//    //}

//    ///// <summary>
//    ///// 움직임 함수
//    ///// </summary>
//    //public ienumerator move()
//    //{
//    //    vector2 curtargetpos;

//    //    for (int nowindex = 0; nowindex < finalnodelist.count; nowindex++)
//    //    {
//    //        curtargetpos.x = finalnodelist[nowindex].xpos;
//    //        curtargetpos.y = finalnodelist[nowindex].ypos;

//    //        while (true)
//    //        {
//    //            if (transform.position.x == curtargetpos.x && transform.position.y == curtargetpos.y)
//    //            {
//    //                break;
//    //            }

//    //            transform.position = vector3.movetowards(transform.position, curtargetpos, time.deltatime * enemydata.speed);

//    //            yield return null;
//    //        }

//    //        if (playercomponent.getcomponent<player>().movetargetpos != targetpos)
//    //        {
//    //            pathfinding();

//    //            yield break;
//    //        }
//    //    }
//    //}

//    //void ondrawgizmos()
//    //{
//    //    if (finalnodelist.count != 0)
//    //    {
//    //        for (int i = 0; i < finalnodelist.count - 1; i++)
//    //        {
//    //            gizmos.drawline(new vector2(finalnodelist[i].xpos, finalnodelist[i].ypos), new vector2(finalnodelist[i + 1].xpos, finalnodelist[i + 1].ypos));
//    //        }
//    //    }
//    //}
//}
