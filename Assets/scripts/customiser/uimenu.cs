using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class uimenu : MonoBehaviour
{
    [SerializeField] private string typ="characters";
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject content;
    [SerializeField] private joiner cosmetics;
    // Start is called before the first frame update
    void Start()
    {
        prefab = Resources.Load("prefabs/UI/icon", typeof(GameObject)) as GameObject;
        content = transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        CreateMenu();
    }

    private void CreateMenu()
    {
        Sprite[] textures = Resources.LoadAll($"images/icons/{typ.ToUpper()}",typeof(Sprite)).Cast<Sprite>().ToArray();
        foreach(Sprite im in textures)
        {
            int id = int.Parse(im.name);
            GameObject img = Instantiate(prefab, content.transform);
            img.GetComponent<Image>().sprite = im;
            Button btn = img.GetComponent<Button>();

            //adding onclick event
            if(typ == "characters")
            {
                btn.onClick.AddListener(delegate { cosmetics.SelectCharacter(id); });
            }
            else
            {
                btn.onClick.AddListener(delegate { cosmetics.SelectCostume(id); });
            }
        }
    }
}
