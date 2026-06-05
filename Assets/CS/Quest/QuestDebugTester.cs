using UnityEngine;

public class QuestDebugTester : MonoBehaviour
{
    private IMainQuestService QuestService => MainQuestManager.Instance;

    public void AcceptMainQuest()
    {
        QuestService?.AcceptMainQuest();
    }

    public void KillSlime()
    {
        QuestService?.OnEnemyKilled(EnemyType.Slime);
    }

    public void KillElite()
    {
        QuestService?.OnEnemyKilled(EnemyType.Elite);
    }

    public void KillBoss()
    {
        QuestService?.OnEnemyKilled(EnemyType.Boss);
    }

    public void ResetQuest()
    {
        MainQuestManager.Instance?.ResetQuest();
    }
}
