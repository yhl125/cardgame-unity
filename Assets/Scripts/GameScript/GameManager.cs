using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Game Buttons
    public Button MenuBtn; // if pressed, leads to main menu

    // Access the player and dealer's script
    public PlayerScript[] players; // current player is always at index 1
    public PlayerScript dealer;

    // public Text to access and update - hud
    public Text moneyText;
    public Text mainText;
    public GameObject hideCard;

    // How much is bet
    int bet = 0;

    void Start()
    {
        // // Add on click listeners to the buttons
        // MainMenuBtn.gameObject.SetActive(false);
        // DepositeBtn.gameObject.SetActive(false);
        // WithdrawBtn.gameObject.SetActive(false);
        // MenuBtn.onClick.AddListener(() => MenuClicked());
        // MainMenuBtn.onClick.AddListener(() => MainMenuClicked());
        // DepositeBtn.onClick.AddListener(() => DepositeClicked());
        // WithdrawBtn.onClick.AddListener(() => WithdrawClicked());
        // for (int i = 0; i < players.Length(); ++i){
        //     players[i].ResetHand();
        // }
        // dealerScript.ResetHand();
        hideCard.GetComponent<Renderer>().enabled = false;
        mainText.gameObject.SetActive(false);
    }

    private void MenuClicked()
    {
        // MainMenuBtn.gameObject.SetActive(true);
        // DepositeBtn.gameObject.SetActive(true);
        // WithdrawBtn.gameObject.SetActive(true);
    }

    private void MainMenuClicked()
    {
        SceneManager.LoadScene("MainMenu"); //될지는 확실하지 않음, 메인메뉴로 이동해야 된다.
    }

    private void WithdrawClicked()
    {
        SceneManager.LoadScene("MainMenu"); //될지는 확실하지 않음
    }
}


// ____________________________ 이 밑에는 다른 코드에서 가져온 부분 ________________________________
//     private void DealClicked()
//     {

//         if (++dealClicks > 1)
//         {
//             scoreText.gameObject.SetActive(true);
//             GameObject.Find("Deck").GetComponent<DeckScript>().Shuffle();
//             playerScript.StartHand();
//             dealerScript.StartHand();
//             // Update the scores displayed
//             scoreText.text = "Hand: " + playerScript.handValue.ToString();
//             dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
//             // Place card back on dealer card, hide card
//             hideCard.GetComponent<Renderer>().enabled = true;
//             // Adjust buttons visibility
//             dealBtn.gameObject.SetActive(false);
//             hitBtn.gameObject.SetActive(true);
//             standBtn.gameObject.SetActive(true);
//             standBtnText.text = "Stand";
//             betBtn.gameObject.SetActive(false);
//             dealClicks = 0;
//         }
//         else
//         {
//             // Reset round, hide text, prep for new hand
//             playerScript.ResetHand();
//             dealerScript.ResetHand();
//             // Hide deal hand score at start of deal
//             dealerScoreText.gameObject.SetActive(false);
//             mainText.gameObject.SetActive(false);
//             dealerScoreText.gameObject.SetActive(false);
//             // Set standard pot size
//             pot = 40;
//             betsText.text = "Bets: $" + pot.ToString();
//             playerScript.AdjustMoney(-20);
//             cashText.text = "$" + playerScript.GetMoney().ToString();
//             betBtn.gameObject.SetActive(true);
//         }
//     }

//     private void HitClicked()
//     {
//         // Check that there is still room on the table
//         if (playerScript.cardIndex <= 10)
//         {
//             playerScript.GetCard();
//             scoreText.text = "Hand: " + playerScript.handValue.ToString();
//             if (playerScript.handValue > 20) RoundOver();
//         }
//     }

//     private void StandClicked()
//     {
//         standClicks++;
//         if (standClicks > 1) RoundOver();
//         HitDealer();
//         standBtnText.text = "Call";
//     }

//     private void HitDealer()
//     {
//         while (dealerScript.handValue < 16 && dealerScript.cardIndex < 10)
//         {
//             dealerScript.GetCard();
//             dealerScoreText.text = "Hand: " + dealerScript.handValue.ToString();
//             if (dealerScript.handValue > 20) RoundOver();
//         }
//     }

//     // Check for winnner and loser, hand is over
//     void RoundOver()
//     {
//         // Booleans (true/false) for bust and blackjack/21
//         bool playerBust = playerScript.handValue > 21;
//         bool dealerBust = dealerScript.handValue > 21;
//         bool player21 = playerScript.handValue == 21;
//         bool dealer21 = dealerScript.handValue == 21;
//         // If stand has been clicked less than twice, no 21s or busts, quit function
//         if (standClicks < 2 && !playerBust && !dealerBust && !player21 && !dealer21) return;
//         bool roundOver = true;
//         // All bust, bets returned
//         if (playerBust && dealerBust)
//         {
//             mainText.text = "All Bust: Bets returned";
//             playerScript.AdjustMoney(pot / 2);
//         }
//         // if player busts, dealer didnt, or if dealer has more points, dealer wins
//         else if (playerBust || (!dealerBust && dealerScript.handValue > playerScript.handValue))
//         {
//             mainText.text = "Dealer wins!";
//         }
//         // if dealer busts, player didnt, or player has more points, player wins
//         else if (dealerBust || playerScript.handValue > dealerScript.handValue)
//         {
//             mainText.text = "You win!";
//             playerScript.AdjustMoney(pot);
//         }
//         //Check for tie, return bets
//         else if (playerScript.handValue == dealerScript.handValue)
//         {
//             mainText.text = "Push: Bets returned";
//             playerScript.AdjustMoney(pot / 2);
//         }
//         else
//         {
//             roundOver = false;
//         }
//         // Set ui up for next move / hand / turn
//         if (roundOver)
//         {
//             hitBtn.gameObject.SetActive(false);
//             standBtn.gameObject.SetActive(false);
//             dealBtn.gameObject.SetActive(true);
//             mainText.gameObject.SetActive(true);
//             dealerScoreText.gameObject.SetActive(true);
//             hideCard.GetComponent<Renderer>().enabled = false;
//             cashText.text = "$" + playerScript.GetMoney().ToString();
//             standClicks = 0;
//             //�÷��̾�� ���� ���ų� $2000 �̻��� ��Ҵٸ� ������ ���� �ɼ��� �־�����.
//             if (playerScript.GetMoney() <= 0 || playerScript.GetMoney() >= 2000)
//             {

//                 endBtn.gameObject.SetActive(true);
//                 restartBtn.gameObject.SetActive(true);
//             }

//         }
//     }

//     // Add money to pot if bet clicked
//     void BetClicked()
//     {
//         Text newBet = betBtn.GetComponentInChildren(typeof(Text)) as Text;
//         int intBet = int.Parse(newBet.text.ToString().Remove(0, 1));
//         playerScript.AdjustMoney(-intBet);
//         cashText.text = "$" + playerScript.GetMoney().ToString();
//         pot += (intBet * 2);
//         betsText.text = "Bets: $" + pot.ToString();
//     }
// }