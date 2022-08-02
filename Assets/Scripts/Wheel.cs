using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public class Wheel : MonoBehaviour
{
    public int numberOfSectors = 8;
    List<int> sectors;
    List<string> rewards;
    bool spinning;
    bool canStop;
    float speed = 500f;
    public string getDataURL = "http://23.240.220.14:3060/roulettegame/chances.php";
    public string postDataURL = "http://23.240.220.14:3060/roulettegame/uploadresults.php";
    private string secretKey = "mySecretKey";

    //ideally in a real scenario player id should be acquired from elsewhere
    string playerId = "p0001";

    Transform target;
    float targetz;

    // Start is called before the first frame update
    void Start()
    {
        sectors = new List<int> { 20, 10, 10, 10, 5, 20, 5, 20 };
        spinning = false;
        canStop = false;
        target = new GameObject().transform;
        targetz = 0f;
        rewards = new List<string> { "Life-30-min", "Brush-3X", "Gems-35", "Hammer-3X", "Coins-750", "Brush-1X", "Gems-75", "Hammer-1X" };
    }

    // Update is called once per frame
    void Update()
    {
        if (spinning)
        {
            if (canStop && (Mathf.Abs(((transform.eulerAngles.z) % 360) - targetz)) < 1)
            {
                spinning = false;
            }
            else
            {
                transform.Rotate(0, 0, speed * Time.deltaTime);
            }
        }
    }

    public void SpinWheel()
    {
        if (!spinning)
            StartCoroutine(PopulateChances(false));
    }

    public void SpinLoadedWheel()
    {
        int sector = GetWheelResultSector();
        target.rotation.Set(0, 0, ((360 / numberOfSectors) * (sector - 0.5f)), target.rotation.w);
        targetz = (360 / numberOfSectors) * (sector - 0.5f);
        canStop = false;
        spinning = true;
        StartCoroutine(LetSpin());
        StartCoroutine(PostSpin(playerId, sector, rewards[sector - 1]));
    }

    public void TestChances()
    {
        StartCoroutine(PopulateChances(true));
    }

    public void TestLoadedChances()
    {
        List<int> results = new List<int> { };
        string summary = "";
        for (int i = 1; i < 1001; i++)
        {
            int sector = GetWheelResultSector();
            Debug.Log("Sector " + sector.ToString());
            results.Add(sector);
        }
        for (int i = 1; i <= sectors.Count; i++)
        {
            int total = 0;
            for (int n = 0; n < results.Count; n++)
            {
                if (results[n] == i)
                    total++;
            }
            summary += "Sector " + i + " Total: " + total.ToString() + "; ";
        }
        Debug.Log(summary);
    }

    IEnumerator LetSpin()
    {
        yield return new WaitForSeconds(2f);
        canStop = true;
    }

    IEnumerator PostSpin(string id, int sector, string reward)
    {
        WWWForm form = new WWWForm();
        form.AddField("id", playerId);
        form.AddField("sector", sector);
        form.AddField("reward", reward);
        //string hash = HashInput(id + sector + reward + secretKey);
        //string post_url = postDataURL + "id=" + UnityWebRequest.EscapeURL(id) + "&sector=" + sector + "&reward=" + reward + "&hash=" + hash;
        UnityWebRequest hs_post = UnityWebRequest.Post(postDataURL, form);
        yield return hs_post.SendWebRequest();
        if (hs_post.error != null)
            Debug.Log("There was an error posting to the database: "
                    + hs_post.error);
        hs_post.Dispose();
    }

    IEnumerator PopulateChances(bool debugMode)
    {
        UnityWebRequest hs_get = UnityWebRequest.Get(getDataURL);
        yield return hs_get.SendWebRequest();
        if (hs_get.error != null)
            Debug.Log("There was an error acessing the database: "
                    + hs_get.error);
        else
        {
            string dataText = hs_get.downloadHandler.text;
            MatchCollection mc = Regex.Matches(dataText, @"_");
            if (mc.Count > 0)
            {
                sectors = new List<int> { };
                rewards = new List<string> { };
                string[] splitData = Regex.Split(dataText, @"_");
                for (int i = 0; i < mc.Count; i++)
                {
                    if (i % 2 == 0)
                        sectors.Add(int.Parse(splitData[i]));
                    else
                        rewards.Add(splitData[i].Substring(0, splitData[i].Length - 2));
                }
                numberOfSectors = sectors.Count;
            }
            hs_get.Dispose();
        }
        if (debugMode)
            TestLoadedChances();
        else
            SpinLoadedWheel();
    }

    public string HashInput(string input)
    {
        SHA256Managed hm = new SHA256Managed();
        byte[] hashValue =
                hm.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
        string hash_convert =
                 System.BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        return hash_convert;
    }

    int GetWheelResultSector()
    {
        int result = Random.Range(1, 100);
        int sumSoFar = 0;
        for (int i = 0; i < numberOfSectors; i++)
        {
            sumSoFar += sectors[i];
            if (result <= sumSoFar)
                return i + 1;
        }
        // as long as all chances sum within 100%, this should never be reachable
        return numberOfSectors;
    }
}
