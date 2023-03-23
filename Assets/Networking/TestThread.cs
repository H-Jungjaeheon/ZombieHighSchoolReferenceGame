using UnityEngine;
using System.Threading;

public class TestThread : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Thread GetThread = new Thread(Test);
        GetThread.Start();

        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"Func {i}");
            Thread.Sleep(5);
        }
    }

    void Test()
    {
        for (int i = 0; i < 10; i++)
        {
            Debug.Log($"Thread {i}");
            Thread.Sleep(5);
        }
    }
}
