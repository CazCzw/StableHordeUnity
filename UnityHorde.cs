 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.IO;
using WebP;
using Siccity.GLTFUtility;


public class UnityHorde : MonoBehaviour
{

    public GameObject downloadedObject;

    public GameObject statusText;
    public GameObject warningCanvas;

    private string Hord_API_key = "0000000000";
    private string url = "https://stablehorde.net/api/v2/generate/sync";
    private int n = 1;
    private int steps = 50;
    private int width = 512;
    private int height = 512;
    [SerializeField]
    private Shader triplanarShader;

    // Start is called before the first frame update
    void Start()
    {
        
        string fname = System.DateTime.Now.ToString("HH-mm-ss") + ".csv";
        string path = Path.Combine(Application.persistentDataPath, fname);
        Debug.Log(path);

    }

    public void setHordeKey(string key){
        Hord_API_key = key;
    }

    public Material GetMaterial(Texture2D texture)
    {
        Material outputMaterial = new Material(triplanarShader);

        outputMaterial.mainTexture = texture;
        return outputMaterial;
    }
    

    public IEnumerator GetImgViaRESTApi(MeshRenderer meshRenderer, string inputString)
    {
        Debug.Log("Sending request to Hord API");

        // set up the request
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        // sample: curl -H "Content-Type: application/json" -H "apikey: 0000000000" -d '{"prompt":"A horde of stable robots", "params":{"n":1, "width": 256, "height": 256}}' https://stablehorde.net/api/v2/generate/sync
        // set up the json data
        string json = "{\"prompt\": \"" + inputString 
                        + "\", \"params\": {\"n\": " + n 
                        + ", \"steps\": " + steps 
                        + ", \"width\": " + width 
                        + ", \"height\": " + height + "}}";

        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(json);

        // set up the headers
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", Hord_API_key);

        // set up the body
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        // send the request
        yield return request.SendWebRequest();

        // check for errors
        if (UnityWebRequest.Result.ConnectionError==request.result || UnityWebRequest.Result.ProtocolError==request.result)
        {
            Debug.Log(request.error); 
            request.Dispose();
        }
        else
        {
            Debug.Log("response Recieved!");
            // get the response
            Response response = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text);
            Generation generation = response.generations[0];

            request.Dispose();

            // convert the image from base64 to bytes
            byte[] bytes = System.Convert.FromBase64String(generation.img);

            // convert the bytes to a texture
            Texture2D texture = new Texture2D(width, height);
            texture = Texture2DExt.CreateTexture2DFromWebP(bytes, lMipmaps: true, lLinear: true, lError: out Error lError);
            // wcp.GetTextureFromBytes(bytes);

            // get material from png file
            Material outputMaterial = GetMaterial(texture);

            // set material
            meshRenderer.material = outputMaterial;
            statusText.SetActive(false);
        }
    }
    
}