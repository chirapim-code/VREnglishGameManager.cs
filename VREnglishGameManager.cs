using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VREnglishGameManager : MonoBehaviour
{
    // ==========================================
    // 1. โครงสร้างข้อมูลโจทย์และผู้เล่น (Data Structures)
    // ==========================================
    [System.Serializable]
    public class Question
    {
        public string[] correctOrder;   // ประโยคที่ถูกต้องแยกเป็นคำๆ
        public string[] scrambledWords; // คำศัพท์ที่สลับที่กันไว้แสดงผล
    }

    [System.Serializable]
    public class PlayerData
    {
        public string name;
        public int score;
    }

    [System.Serializable]
    public class LeaderboardData
    {
        public List<PlayerData> list = new List<PlayerData>();
    }

    // ==========================================
    // 2. ตัวแปรสำหรับเชื่อมต่อหน้าจอ UI (UX/UI Elements)
    // ==========================================
    [Header("Screens")]
    public GameObject startScreen;
    public GameObject gameScreen;
    public GameObject endScreen;

    [Header("Input & Text Elements")]
    public TMP_InputField nameInputField;
    public TMP_Text questionProgressText;
    public TMP_Text scoreText;
    public TMP_Text feedbackText; 
    public TMP_Text leaderboardText; // บอร์ดแสดงอันดับคะแนนพาสเทลสดใส

    [Header("VR Interaction Containers")]
    public Transform wordContainer;      // จุดที่กล่องคำศัพท์ลอยขึ้นมาให้เลือก
    public Transform dropZoneContainer;  // จุดที่ผู้เล่นนำคำศัพท์มาเรียงต่อกัน

    [Header("Prefabs")]
    public GameObject wordBlockPrefab;   // พรีแฟบบล็อก 3D หรือปุ่มคำศัพท์

    // ==========================================
    // 3. ตัวแปรระบบเกม (Internal Variables)
    // ==========================================
    private List<Question> questions = new List<Question>();
    private int currentQuestionIndex = 0;
    private int currentScore = 0;
    private string playerName = "";
    private string saveKey = "VR_English_Leaderboard_Data";

    // ==========================================
    // 4. ระบบการควบคุมการเล่นเกม (Gameplay Logic)
    // ==========================================
    void Start()
    {
        ShowScreen(startScreen);
        SetupQuestions(); // โหลดโจทย์ที่เตรียมไว้ทั้ง 10 ข้อ
    }

    // ฟังก์ชันกดเริ่มเกมหลังจากพิมพ์ชื่อเสร็จ
    public void StartGame()
    {
        playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Student";

        currentQuestionIndex = 0;
        currentScore = 0;
        
        ShowScreen(gameScreen);
        LoadQuestion(currentQuestionIndex);
    }

    // ฟังก์ชันสร้างโจทย์คำศัพท์ลอยขึ้นมาในฉาก VR
    void LoadQuestion(int index)
    {
        feedbackText.text = "";
        questionProgressText.text = $"ข้อที่: {index + 1} / {questions.Count}";
        scoreText.text = $"คะแนนปัจจุบัน: {currentScore}";

        // เคลียร์กล่องข้อความคำศัพท์เก่าในฉากออกทั้งหมด
        foreach (Transform child in wordContainer) Destroy(child.gameObject);
        foreach (Transform child in dropZoneContainer) Destroy(child.gameObject);

        // สุ่มสร้างกล่องคำศัพท์ที่สลับที่แล้วในจุดเลือกคำ
        foreach (string word in questions[index].scrambledWords)
        {
            GameObject block = Instantiate(wordBlockPrefab, wordContainer);
            block.GetComponentInChildren<TMP_Text>().text = word;
            // หมายเหตุ: ใน VR อย่าลืมใส่ Component XRGrabInteractable ที่พรีแฟบบล็อกด้วยนะครับ เพื่อให้ใช้นิ้วคีบได้
        }
    }

    // ฟังก์ชันสำหรับปุ่มตรวจคำตอบ
    public void CheckAnswer()
    {
        // ตรวจสอบว่าในโซนรับคำตอบมีการดึงบล็อกคำศัพท์ไปวางกี่คำแล้ว
        TMP_Text[] placedWords = dropZoneContainer.GetComponentsInChildren<TMP_Text>();
        Question currentQ = questions[currentQuestionIndex];

        if (placedWords.Length != currentQ.correctOrder.Length)
        {
            feedbackText.text = "<color=#FF5555>ยังย้ายกล่องคำศัพท์ไม่ครบเลยจ้า!</color>";
            return;
        }

        // เช็คลำดับตัวอักษรกล่องต่อกล่องจากซ้ายไปขวา
        bool isCorrect = true;
        for (int i = 0; i < placedWords.Length; i++)
        {
            if (placedWords[i].text != currentQ.correctOrder[i])
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            currentScore += 10;
            feedbackText.text = "<color=#22FF22>ถูกต้องแล้ว! เก่งมาก ✨</color>";
            Invoke("NextQuestion", 2f); // ให้เด็กๆ ดีใจ 2 วิก่อนข้ามข้อ
        }
        else
        {
            string correctAnswerString = string.Join(" ", currentQ.correctOrder);
            feedbackText.text = $"<color=#FF5555>ยังไม่ถูกน้า ลองดูประโยคนี้สิ:</color>\n<color=#FFFF55>{correctAnswerString}</color>";
            Invoke("NextQuestion", 4f); // หน่วงเวลาเพิ่มขึ้น เพื่อให้นักเรียนได้อ่านทวนคำตอบที่ถูกต้อง
        }
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        if (currentQuestionIndex < questions.Count)
        {
            LoadQuestion(currentQuestionIndex);
        }
        else
        {
            EndGame();
        }
    }

    void EndGame()
    {
        ShowScreen(endScreen);
        SaveAndDisplayLeaderboard(playerName, currentScore);
    }

    void ShowScreen(GameObject screenToShow)
    {
        startScreen.SetActive(screenToShow == startScreen);
        gameScreen.SetActive(screenToShow == gameScreen);
        endScreen.SetActive(screenToShow == endScreen);
    }

    // ==========================================
    // 5. ระบบเซฟบันทึกสถิติและกระดานคะแนน (Leaderboard System)
    // ==========================================
    void SaveAndDisplayLeaderboard(string name, int score)
    {
        // โหลดคะแนนเก่าขึ้นมาจากเครื่อง
        string json = PlayerPrefs.GetString(saveKey, "");
        LeaderboardData data = string.IsNullOrEmpty(json) ? new LeaderboardData() : JsonUtility.FromJson<LeaderboardData>(json);

        // เพิ่มชื่อและคะแนนล่าสุดเข้าไป
        PlayerData newPlayer = new PlayerData { name = name, score = score };
        data.list.Add(newPlayer);

        // จัดเรียงคะแนนจากมากไปหาน้อย
        data.list.Sort((x, y) => y.score.CompareTo(x.score));

        // คัดเอาเฉพาะ Top 5 อันดับแรกเพื่อไม่ให้บอร์ดคะแนนยาวยืดจนเกินไป
        if (data.list.Count > 5)
        {
            data.list.RemoveRange(5, data.list.Count - 5);
        }

        // เซฟทับลงระบบความจำเครื่อง
        string newJson = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(saveKey, newJson);
        PlayerPrefs.Save();

        // แสดงผลลัพธ์รายชื่อลงบนบอร์ดคะแนนในโลก VR (รองรับแท็กสีสันสดใส)
        leaderboardText.text = "<size=130%><color=#FFDD00>🏆 อันดับนักเรียนคนเก่ง 🏆</color></size>\n\n";
        
        for (int i = 0; i < data.list.Count