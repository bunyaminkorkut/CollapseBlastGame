using UnityEngine;
using UnityEngine.UI;

public class Block : MonoBehaviour
{
    public int row;
    public int col;
    public Color color;

    private SpriteRenderer spriteRenderer; 

    public Sprite DefaultIcon;
    public Sprite AIcon;
    public Sprite BIcon;
    public Sprite CIcon;




    private GridManager gridManager;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        GetComponent<SpriteRenderer>().sprite = DefaultIcon;
        GetComponent<SpriteRenderer>().color = Color.white;
    }


    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0); 
            if (touch.phase == TouchPhase.Began)
            {
                Vector3 touchPos = Camera.main.ScreenToWorldPoint(touch.position);
                touchPos.z = 0; 

                if (GetComponent<Collider2D>().OverlapPoint(touchPos))
                {
                    HandleClick();
                }
            }
        }
    }

    private void OnMouseDown()
    {
        HandleClick();
    }

    private void HandleClick()
    {
        Block gridBlock = gridManager.GetBlock(row, col);
        gridManager.BlastBlocks(row, col);
    }

    public void SetIcon(Sprite newIcon)
    {
        if (newIcon != null)
        {
            GetComponent<SpriteRenderer>().sprite = newIcon;
        }
    }

}