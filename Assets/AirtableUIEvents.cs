using Airtable;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.String;

public class AirtableUIEvents : MonoBehaviour {

    public TextMeshProUGUI textCompanyName;
    public TextMeshProUGUI textCheck;
    public TextMeshProUGUI textTileName;
    public TextMeshProUGUI selectedTileInForum;

    public TMP_InputField fieldCompanyName;
    public TMP_InputField fieldCompanyApprev;
    public TMP_InputField fieldContactName;
    public TMP_InputField fieldContactEmail;
    public TMP_InputField fieldNotes;
    public Button buttonTile;

    public GameObject buttonPanel;
    public GameObject forumPanel;

    private AirtableClient api;

    private TMP_InputField[] RequiredFields;
    private TMP_InputField[] OptionalFields;

    public Dictionary<string, Dictionary<string, string>> tileRecords;
    
    private const string CheckOutText = "Check out tile!";
    private const string OwnedTileText = "Checked out by:";
    private const string AirTableTileID = "Tile ID";
    private const string AirTableCompanyName = "Company Name";

    private const string API_KEY = "keyRInfLRPUnIip0L";
    private const string APP_ID = "appiXA1PKq1wFA7yi";

    private string _currentTile;
    private string _selectedTile;

    public string CurrentTile {
        private get => _currentTile;
        set
        {
            _currentTile = value;
            textTileName.text = _currentTile;
            //check to see if tile is documented 
            tileRecords.TryGetValue(_currentTile, out var tempRecord);
            if (tempRecord != null && tempRecord[AirTableCompanyName] != Empty) {
                textCheck.text = OwnedTileText;
                textCompanyName.text = tempRecord[AirTableCompanyName];
                buttonTile.interactable = false;
            } else {
                textCheck.text = CheckOutText;
                textCompanyName.text = "Available";
                buttonTile.interactable = true;
            }
        }
    }
    
    

    public void Awake()
        {
            api = new AirtableClient(API_KEY, APP_ID);

            RequiredFields = new TMP_InputField[] {fieldCompanyName, fieldCompanyApprev, fieldContactName, fieldContactEmail};

            OptionalFields = new TMP_InputField[] {fieldNotes};

            ListAirtableRecords();
            ShowButtonLayout();
        }

    public async void ListAirtableRecords()
        {
            var result = await api.ListRecords();

            tileRecords = new Dictionary<string, Dictionary<string, string>>();
            foreach (var record in result) {
                string tileKey = record[AirTableTileID].Insert(1, "-");
                tileRecords[tileKey] = record;
            }

            string tileKeys = "";
            foreach (var tile in tileRecords) {
                tileKeys = tileKeys + " " + tile.Key;
            }

            Debug.Log(tileKeys);
        }

    private void ResetForums()
        {
            // Reset form values
            foreach (var field in RequiredFields) {
                field.text = "";
            }
            foreach (var field in OptionalFields) {
                field.text = "";
            }
            _selectedTile = "";
        }

    public void ShowForumLayout()
        {
            ResetForums();

            _selectedTile = CurrentTile;
            selectedTileInForum.text = _selectedTile;

            buttonPanel.SetActive(false);
            forumPanel.SetActive(true);
        }

    public void ShowButtonLayout()
        {
            ResetForums();
            buttonPanel.SetActive(true);
            forumPanel.SetActive(false);
        }

    public async void SubmitCreateRecordForm()
        {
            var record = new Dictionary<string, string> {[AirTableTileID] = _selectedTile.Replace("-", Empty) };
            Debug.Log(_selectedTile);

            foreach (var field in RequiredFields) {
                var value = field.text;
                if (value == "") {
                    Debug.Log(field.name + " is required");
                    return;
                } else {
                    record[field.name] = value;
             
                }
            }
            

            foreach (var field in OptionalFields) {
                record[field.name] = field.text.ToString();
                Debug.Log(record[field.name]);
            }

            
            string body = await api.CreateRecord(record);

            // Call the List method for refreshing the records
            ListAirtableRecords();

            ShowButtonLayout();
        }

}