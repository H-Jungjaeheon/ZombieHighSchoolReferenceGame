using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    public static StageManager instance;

    [SerializeField]
    [Tooltip("���� ǥ�� �ؽ�Ʈ ������Ʈ")]
    private Text scoreText;

    [SerializeField]
    [Tooltip("���� ����")]
    private int curGameScore;

    private void Awake()
    {
        if (instance = null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// ���� ���ھ� 
    /// </summary>
    /// <param name="plusValue"> ���� ���� </param>
    public void PlusScore(int plusValue)
    {
        curGameScore += plusValue;
        scoreText.text = $"{curGameScore}";
    }
}
