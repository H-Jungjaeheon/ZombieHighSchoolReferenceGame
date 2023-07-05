//using system.collections;
//using system.collections.generic;
//using unityengine;
//using unityengine.ui;

//[system.serializable]
//public class node
//{
//    /// <summary>
//    /// ��� ������
//    /// </summary>
//    /// <param name="_iswall"> ���� ������ �Ǻ� </param>
//    /// <param name="x"> ����� x ������ </param>
//    /// <param name="y"> ����� y ������ </param>
//    public node(bool _iswall, int x, int y)
//    {
//        iswall = _iswall;

//        nodepos.x = x;
//        nodepos.y = y;
//    }

//    [tooltip("���� ��尡 ������ �Ǻ�")]
//    public bool iswall;

//    [tooltip("���� ����� �θ� ���(���� ���)")]
//    public node parentnode;

//    [tooltip("���� ����� ������")]
//    public vector2int nodepos;

//    [tooltip("�������κ��� �̵��� �Ÿ�")]
//    public int gcost;

//    [tooltip("��ֹ��� ������ ��ǥ������ �Ÿ� (����, ����)")]
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
//    [tooltip("�⺻ �� ����(scriptableobject)")]
//    protected enemydata enemydata;

//    [tooltip("ü��")]
//    private int hp;

//    [serializefield]
//    [tooltip("ü�¹� �̹��� ������Ʈ")]
//    protected image hpbarimg;

//    [serializefield]
//    [tooltip("spriterenderer ������Ʈ")]
//    protected spriterenderer sr;

//    [serializefield]
//    [tooltip("���� ������ �÷��̾��� player ������Ʈ")]
//    protected player playercomponent;

//    [tooltip("�ڷ�ƾ ������ : 0.1��")]
//    private waitforseconds zerotoone = new waitforseconds(0.1f);

//    [tooltip("���� �������� Ÿ�� ȿ�� �ڷ�ƾ")]
//    private ienumerator hiteffectcoroutine;

//    #region �̵� ���� ��ҵ� ����
//    [header("�̵� ���� ��ҵ� ����")]

//    const int move_straight_cost = 10;
//    const int move_diagonal_cost = 14;

//    [tooltip("���� �� ũ�� ����� vector")]
//    private vector2int mapsize;

//    [tooltip("���� �� ���� Ÿ�� ��ġ vector (�� ���� �Ʒ� Ÿ��)")]
//    private vector2int criteriatilepos;

//    //[tooltip("�߰� �� �÷��̾� ���� ����")]
//    //private const int updatedetectedrange = 50;

//    [tooltip("���� ��尡 ������ �Ǻ�")]
//    private bool iswall;

//    [tooltip("��� Ž�� �� ���� ������")]
//    private vector3 startpos;

//    [tooltip("��� Ž�� �� ��ǥ ������")]
//    private vector3 targetpos;

//    [tooltip("���� ��� ��� ����Ʈ")]
//    public list<node> finalnodelist;

//    //[tooltip("��ü ��� ���� ��� �迭")]
//    //private node[,] nodearray = new node[updatedetectedrange * 2 + 1, updatedetectedrange * 2 + 1];

//    [tooltip("��� ���� ���")]
//    private node startnode;

//    [tooltip("������ ���")]
//    private node targetnode;

//    //[tooltip("���� ��� ���")]
//    //private node curnode;

//    [tooltip("��� Ž�� ������ ��� ����Ʈ")]
//    private list<node> openlist = new list<node>();

//    private list<node> mapdata = new list<node>();

//    //[tooltip("��� Ž�� �Ϸ��� ��� ����Ʈ")]
//    //private list<node> closedlist = new list<node>();

//    [tooltip("�� �±�")]
//    private const string wall = "wall";
//    #endregion

//    private void awake()
//    {
//        startsetting();
//    }

//    /// <summary>
//    /// ���� ���� �Լ�
//    /// </summary>
//    private void startsetting()
//    {
//        hp = enemydata.maxhp;

//        mapsize = new vector2int(87, 66); //���� �Ŵ������� ���� ���������� �´� �� ũ�� �޾ƿ��� ������ ����
//        criteriatilepos = new vector2int(-35, -37);
//    }

//    /// <summary>
//    /// �ǰ� �Լ�
//    /// </summary>
//    /// <param name="damage"> ���� ������ </param>
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
//    /// Ÿ�� ȿ�� �Լ�
//    /// </summary>
//    /// <returns></returns>
//    private ienumerator hiteffect()
//    {
//        sr.color = color.red;

//        yield return zerotoone;

//        sr.color = color.white;
//    }

//    /// <summary>
//    /// ��� �Լ�
//    /// </summary>
//    private void dead()
//    {
//        stagemanager.instance.plusscore(enemydata.score);

//        destroy(gameobject);
//    }

//    /// <summary>
//    /// �÷��̾� ���� ����
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

//        #region  �ʰ�ȭ 

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


//            #region  �ð�ȭ �ϱ� ���� �� ���� ����Ʈ�� �� ��带 �ʷ����� ����
//            if (currentnode != startnode)
//            {
//                currentnode.tile.setcolor(color.green);
//            }
//            #endregion

//            if (currentnode == endnode) // ���� ��尡 �� �����
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

//            currentnode.moveable = false;    // �ѹ� �� ��� 

//            yield return pointzerofivesec;
//        }
//    }

//    #region  ������ �˻�

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

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x + 1, y) == false)   // Ž�� ���� ����
//            {
//                break;
//            }
//            currentnode = getgrid(++x, y); // ���� ������ ���� �̵�

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

//            #region ������ ���� �����鼭 ������ ���� �շ��ִ� ���

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y + 1).moveable == false) // ������ ���̸�
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // ������ ���� �������� ������
//                    {
//                        if (ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        isfind = true;
//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region ������ ���� �����鼭 ������ �Ʒ��� �շ��ִ� ���

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y - 1).moveable == false) // �Ʒ����� ���̰�
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) // ������ �Ʒ��� ���� ���� ������
//                    {
//                        if (ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        isfind = true;
//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion
//        }

//        startnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  ���� �˻�

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


//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x - 1, y) == false)   // Ž�� ���� ����
//            {
//                break;
//            }

//            currentnode = getgrid(--x, y); // ���� ���� ���� �̵�
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

//            #region ������ ���� �����鼭 ���� ���� �շ��ִ� ���

//            if (y + 1 < mapsize.y && x - 1 >= 0)
//            {
//                if (getgrid(x, y + 1).moveable == false) // ������ ���̸�
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // ���� ���� �������� ������
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���

//            if (y > 0 && x - 1 >= 0)
//            {
//                if (getgrid(x, y - 1).moveable == false) // �Ʒ����� ���̰�
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // ���� �Ʒ��� ���� ���� ������
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }
//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion
//        }

//        startnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  ���� �˻�

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

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x, y + 1) == false)   // Ž�� ���� ����
//            {
//                break;
//            }

//            currentnode = getgrid(x, ++y); // ���� ���� ���� �̵�
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

//            #region ������ ���� �����鼭 ���� ���� �շ��ִ� ���

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // �������� ���̸�
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // ������ ����
//                    {
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        isfind = true;
//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���

//            if (y + 1 < mapsize.y && x > 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // ������ ���̰�
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // ���� ���� ���� ���� ������
//                    {
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        isfind = true;
//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion
//        }

//        _satrtnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  �Ʒ��� �˻�

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

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x, y - 1) == false)   // Ž�� ���� ����
//            {
//                break;
//            }

//            currentnode = getgrid(x, --y); // ���� ���� ���� �̵�
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

//            #region ������ ���� �����鼭 ���� ���� �շ��ִ� ���

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // �������� ���̸�
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) // ������ �Ʒ��� �� �� �ִٸ�
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region ������ ���� �����鼭 ���� �Ʒ��� �շ��ִ� ���

//            if (y > 0 && x - 1 >= 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // ������ ���̰�
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // ���� �Ʒ��� ���� ���� ������
//                    {
//                        isfind = true;
//                        if (_ismovealbe == false)
//                        {
//                            addopenlist(currentnode, startnode);
//                        }

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion
//        }

//        _satrtnode.moveable = true;
//        return isfind;
//    }

//    #endregion


//    #region  ������ �� �밢�� �˻�

//    private void rightup(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;


//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = false;

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x + 1, y + 1) == false)   // Ž�� ���� ����
//            {
//                break;
//            }



//            currentnode = getgrid(++x, ++y); // ���� ���� ���� �̵�

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

//            #region ������ ���� �����鼭 ���� ���� �������� ���� ���

//            if (y + 1 < mapsize.y && x > 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // ������ ����
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // ���� ���� �ȸ���
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region �Ʒ��� ���� �����鼭 ������ �Ʒ��� �ȸ�������

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y - 1).moveable == false) // ������ ���̰�
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) // ���� �Ʒ��� ���� ���� ������
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
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


//    #region  ������ �Ʒ� �밢�� �˻�

//    private void rightdown(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;

//        while (currentnode.moveable == true)
//        {

//            currentnode.moveable = false;

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x + 1, y - 1) == false)   // Ž�� ���� ����
//            {
//                break;
//            }

//            currentnode = getgrid(++x, --y); // ���� ���� ���� �̵�

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

//            #region ������ �����ְ� ���� �Ʒ��� ���� ���� ������

//            if (y > 0 && x > 0)
//            {
//                if (getgrid(x - 1, y).moveable == false) // ������ ����
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // ���� ���� �ȸ���
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region ������ �����ְ� ������ ���� �������� ������

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x, y + 1).moveable == false) // ������ ���̰�
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // ������ ���� �������� ������
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
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


//    #region  ���� �� �밢�� �˻�

//    private void leftup(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;


//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = false;

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x - 1, y + 1) == false)   // Ž�� ���� ����
//            {
//                break;
//            }

//            currentnode = getgrid(--x, ++y); // ���� ���� ���� �̵�

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

//            #region �������� �����ְ� ������ ���� �������� ������

//            if (y + 1 < mapsize.y && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // �������� ����
//                {
//                    if (getgrid(x + 1, y + 1).moveable == true) // ������ ���� �ȸ���
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region �Ʒ��� �����ְ� ���� �Ʒ��� �������� ������

//            if (y > 0 && x > 0)
//            {
//                if (getgrid(x, y - 1).moveable == false) // �Ʒ��� ��
//                {
//                    if (getgrid(x - 1, y - 1).moveable == true) // ���� �Ʒ��� �ȸ���
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
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

//    #region  ���� �Ʒ� �밢�� �˻�

//    private void leftdown(node _satrtnode)
//    {
//        int x = _satrtnode.position.x;
//        int y = _satrtnode.position.y;

//        node startnode = _satrtnode;
//        node currentnode = startnode;


//        while (currentnode.moveable == true)
//        {
//            currentnode.moveable = false;

//            #region �� ����� �Ѿ���� �� �Ѿ�������� üũ

//            if (comp(x - 1, y - 1) == false)   // Ž�� ���� ����
//            {
//                break;
//            }

//            currentnode = getgrid(--x, --y); // ���� ���� ���� �̵�

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

//            #region �������� �����ְ� ������ ���� �������� ������

//            if (y > 0 && x + 1 < mapsize.x)
//            {
//                if (getgrid(x + 1, y).moveable == false) // �������� ����
//                {
//                    if (getgrid(x + 1, y - 1).moveable == true) //������ �Ʒ��� �� ����
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
//                    }
//                }
//            }

//            #endregion

//            #region �Ʒ��� �����ְ� ���� �Ʒ��� �������� ������

//            if (y + 1 < mapsize.y && x < 0)
//            {
//                if (getgrid(x, y + 1).moveable == false) // ���� ����
//                {
//                    if (getgrid(x - 1, y + 1).moveable == true) // ���� ���� �ȸ���
//                    {
//                        addopenlist(currentnode, startnode);

//                        break; // �ڳ� �߰��ϸ� �ٷ� ����
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

//    #region  Ž�� ���� ����
//    private bool comp(int _x, int _y)
//    {
//        if (_x < 0 || _y < 0 || _x >= mapsize.x || _y >= mapsize.y)
//        {
//            return false;
//        }
//        return true;
//    }
//    #endregion


//    #region ����Ʈ �ȿ� �ִ� ��带 x�� y��ǥ ������ ��ȯ

//    private node getgrid(int _x, int _y)
//    {
//        return mapdata[_x + _y * mapsize.x];
//    }

//    #endregion


//    /// <summary>
//    /// �� ���� �Լ�
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


//    #region ���� ������ ���� ª�� ����� ���� ��ȯ �޸���ƽ
//    private int heuristic(vector2int _currposition, vector2int _endposition)
//    {
//        int x = mathf.abs(_currposition.x - _endposition.x);
//        int y = mathf.abs(_currposition.y - _endposition.y);
//        int reming = mathf.abs(x - y);

//        return move_diagonal_cost * mathf.min(x, y) + move_straight_cost * reming;
//    }

//    #endregion


//    #region  ����Ʈ �߿� ���� ª�� f���� ���� ��带 ��ȯ
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
//    ///// ��� Ž�� �Լ�
//    ///// </summary>
//    //public void pathfinding()
//    //{
//    //    startpos = transform.position;
//    //    targetpos = playercomponent.movetargetpos;

//    //    for (int i = -updatedetectedrange; i <= updatedetectedrange; i++) //�÷��̾� �߰� ������ŭ ��� ����
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
//    //        // ���¸���Ʈ �� ���� f�� ���� ��, f�� ���ٸ� h�� ���� ��, h�� ���ٸ� 0��° ���� ������� �ϰ� ��������Ʈ���� ��������Ʈ�� �ű��
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

//    //        // ������
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
//    ///// ���� ������ ����� ���� ���¸���Ʈ �߰� �Լ�
//    ///// </summary>
//    ///// <param name="checkx"></param>
//    ///// <param name="checky"></param>
//    //private void openlistadd(int checkx, int checky)
//    //{
//    //    //���� ������ ����� �ʰ�, ���� �ƴϸ鼭, ��������Ʈ�� ���ٸ�
//    //    if (checkx >= startpos.x - updatedetectedrange && checkx <= startpos.x + updatedetectedrange
//    //        && checky >= startpos.y - updatedetectedrange && checky <= startpos.y + updatedetectedrange
//    //        && nodearray[(int)(checkx - startpos.x) + updatedetectedrange, (int)(checky - startpos.y) + updatedetectedrange].iswall == false
//    //        && closedlist.contains(nodearray[(int)(checkx - startpos.x) + updatedetectedrange, (int)(checky - startpos.y) + updatedetectedrange]) == false)
//    //    {
//    //        // �̿���忡 �ֱ�
//    //        node neighbornode = nodearray[(int)(checkx - startpos.x) + updatedetectedrange, (int)(checky - startpos.y) + updatedetectedrange];
//    //        int movecost = 10;

//    //        // �̵������ �̿���� g���� �۰ų� �Ǵ� ��������Ʈ�� �̿���尡 ���ٸ� g, h, parentnode�� ���� �� ��������Ʈ�� �߰�
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
//    ///// ������ �Լ�
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
