//using UnityEngine;

//public class PackAPunchMachine : MonoBehaviour
//{
    //public float interactionDistance = 3f;
    //public int cost = 500;
   // public Transform player; // Referência ao transform do jogador

   // void Update()
   // {
        // Verifica se o jogador está perto e pressionou a tecla de interação (ex: 'E')
       // if (Vector3.Distance(transform.position, player.position) <= interactionDistance && Input.GetKeyDown(KeyCode.E))
       // {
            // Tenta aprimorar a arma
            //AttemptPackAPunch();
        //}
    //}

    //void AttemptPackAPunch()
    //{
        // Pega o script do gerenciador de pontos do jogador
       // PointManager pointManager = PointManager.Instance;

        // Verifica se o jogador tem pontos suficientes
       // if (pointManager != null && pointManager.currentPoints >= cost)
       // {
            // Tenta encontrar o script da arma na hierarquia do jogador
            //AssaultRifle weapon = player.GetComponentInChildren<AssaultRifle>();

           // if (weapon != null)
          //  {
                // Verifica se a arma já foi aprimorada
               // if (!weapon.isPackAPunched)
               // {
                    // Debita os pontos e aprimora a arma
                   // pointManager.SubtractPoints(cost);
                   // weapon.PackAPunchWeapon();
                   // Debug.Log("Arma aprimorada com sucesso!");
               // }
                //else
               // {
                  //  Debug.Log("Esta arma já foi aprimorada!");
               // }
           // }
           // else
           // {
            //    Debug.Log("Nenhuma arma encontrada para aprimorar.");
            //}
        //}
        //else
       // {
           // Debug.Log("Pontos insuficientes para usar o Pack-a-Punch!");
        //}
   //}
//}