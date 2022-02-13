using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameController GameControllerRef { get; private set; }

    public Image DisplayContent;

    public GameItem GameItemOwner { get; private set; }

    /// <summary>
    /// optional
    /// </summary>
    public Button ReadAloudBtn;

    private Transform originalParent;

    public bool ActiveInCheckList = true;

    public bool CanDrag = false;

    /// <summary>
    /// optional
    /// </summary>
    public GameObject CrossoutLine;

    /// <summary>
    /// cache when I am hovering over drop zone
    /// </summary>
    private DropZone hoveringDropZone;

    private int dragTouchID = -1;

    /// <summary>
    /// help determine the draggable status
    /// </summary>
    private bool interactable;

    void Awake()
    {
        originalParent = transform.parent;
    }

    void OnDestroy()
    {
        GameItemOwner = null;
    }

    void Update()
    {
        // temp hack to distingush the right panel buttons from left
        if (GameControllerRef)
        {
            // if we are dragging other things
            interactable = !GameControllerRef.GamePaused;

            // special case to disable other 
            if (CanDrag)
            {
                interactable = false; 

                if (GameControllerRef.DraggedItem == null && !GameControllerRef.GamePaused)
                {
                    interactable = true;
                }
                else if (GameControllerRef.DraggedItem == GameItemOwner && !GameControllerRef.GamePaused)
                {
                    interactable = true;
                }
            }

            DisplayContent.color = interactable ? Color.white : new Color(1, 1, 1, 0.5f);
        }
    }

    public void InitDragIcon(GameItem owner, GameController gameController)
    {
        GameItemOwner = owner;
        DisplayContent.sprite = GameItemOwner.DisplaySprite;

        GameControllerRef = gameController;
    }

    /// <summary>
    /// when cross checked and removed
    /// </summary>
    public void CrossOutCheckListEffect()
    {
        ActiveInCheckList = false;
        //GetComponent<CanvasGroup>().alpha = 0.2f;
        CrossoutLine.SetActive(true);
    }

    public void InitChineseText(GameItem owner, GameController gameController)
    {
        GameItemOwner = owner;

        DisplayContent.sprite = GameItemOwner.DisplayChineseText;

        if(GameItemOwner && ReadAloudBtn)
        {
            ReadAloudBtn.GetComponent<AudioSource>().clip = GameItemOwner.ReadAloudClip;
            ReadAloudBtn.gameObject.SetActive(true);
            DisplayContent.gameObject.SetActive(true);
        }

        GameControllerRef = gameController;

        ActiveInCheckList = true;

        if(CrossoutLine)
        {
            CrossoutLine.SetActive(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Input.multiTouchEnabled = false;

        if(!interactable)
        {
            return;
        }

        if (Input.touchSupported)
        {
            if (Input.touchCount > 1)
            {
                OnEndDrag(null);
                return;
            }
        }

        if (GameControllerRef.DraggedItem == GameItemOwner && CanDrag)
        {
            // disallow other duplicate touches
            GetComponent<Image>().raycastTarget = false;

            Vector2 targetPos = Input.touchSupported && Input.touchCount > 0 ? Input.GetTouch(0).position : new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            transform.position = Camera.main.ScreenToWorldPoint(targetPos);
            transform.position = new Vector3(transform.position.x, transform.position.y + 0.8f, 0);

            transform.SetParent(GameControllerRef.UIFrontLayerRoot.transform);
            transform.SetAsLastSibling();

            if (hoveringDropZone)
            {
                hoveringDropZone.OnDragHoverExit();
            }

            // notify the objects I am hovering
            if (eventData.pointerEnter)
            {
                DropZone shopCartDropZone = eventData.pointerEnter.GetComponent<DropZone>();
                if(shopCartDropZone)
                {
                    hoveringDropZone = shopCartDropZone;

                    shopCartDropZone.OnDragHoverEnter();
                }
            }
        }
    }

    ///// <summary>
    ///// To counter multi touch, get the original touch position
    ///// </summary>
    //private Vector2 GetOriginalTouchPos()
    //{
    //    for(int index = 0; index < Input.touches.Length; ++index)
    //    {
    //        if(Input.touches[index].fingerId == dragTouchID)
    //        {
    //            return Input.touches[index].position;
    //        }
    //    }

    //    return new Vector2(0, 0);
    //}

    public void OnEndDrag(PointerEventData eventData)
    {
       //// Input.multiTouchEnabled = false;
       // if (!interactable)
       // {
       //     return;
       // }

        if (CanDrag)
        {
            GameControllerRef.DraggedItem = null;
            transform.SetParent(originalParent);
            transform.localPosition = Vector3.zero;
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            GetComponent<Image>().raycastTarget = true;

            if (hoveringDropZone)
            {
                hoveringDropZone.OnDragHoverExit();
            }

            dragTouchID = -1;

            //GameControllerRef.DebugText.text = "End Drag" + Input.touchCount;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Input.multiTouchEnabled = false;
        if (!interactable)
        {
            return;
        }

        if (GameControllerRef.DraggedItem == null && CanDrag)
        {
            GameControllerRef.DraggedItem = GameItemOwner;
            transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);

            if (Input.touchSupported)
            {
                dragTouchID = Input.GetTouch(0).fingerId;

                //GameControllerRef.DebugText2.text = "BeginDrag " + dragTouchID;
            }
        }
    }
}
