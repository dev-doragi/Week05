using UnityEngine;

public class GameManager : Singleton<GameManager>
{


    protected override void Init()
    {
        
    }
    void Start()
    {

        GameFlowManager.Instance?.BeginFlow();
    }



  

    
}
