using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [SerializeField]
    [Tooltip("점수 표기 텍스트 컴포넌트")]
    private Text scoreText;

    [SerializeField]
    [Tooltip("현재 점수")]
    private int curGameScore;

    private void Awake()
    {
        if (instance = null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// 현재 스코어 
    /// </summary>
    /// <param name="plusValue"> 더할 점수 </param>
    public void PlusScore(int plusValue)
    {
        curGameScore += plusValue;
        scoreText.text = $"{curGameScore}";
    }
}
