using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionArrow : MonoBehaviour
{
    [SerializeField] private RectTransform[] options;
    public RectTransform[] Options => options;
    [SerializeField] private AudioClip changeSound;
    [SerializeField] private AudioClip interactSound;

    [SerializeField] private bool useGridNavigation = false;
    [SerializeField] private int gridColumns = 3;

    [Header("Skill Info UI")]
    [SerializeField] private GameObject skillUI;
    [SerializeField] private Skill[] skills;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private RectTransform closeButton;

    private RectTransform rect;
    private int currentPosition;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Start()
    {
        if (skillUI != null && skillUI.activeInHierarchy)
        {
            UpdateSkillInfo();
        }
    }

    private void Update()
    {
        if (!useGridNavigation)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                ChangePosition(-1);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                ChangePosition(1);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                ChangeGridPosition(-gridColumns); // move up
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                ChangeGridPosition(gridColumns); // move down
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                ChangeGridPosition(-1); // move left
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                ChangeGridPosition(1); // move right
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E)) //removed  "|| Input.GetKeyDown(KeyCode.Space)" since it conflicts with jump (SFX plays)
            Interact();
    }

    private void ChangePosition(int _change)
    {
        currentPosition += _change;

        if(_change !=0)
            SoundManager.instance.PlaySound(changeSound);

        if (currentPosition < 0)
            currentPosition = options.Length - 1;
        else if (currentPosition > options.Length - 1)
            currentPosition = 0;

        rect.position = new Vector3(options[currentPosition].position.x - 250, options[currentPosition].position.y, 0);
    }

    private void ChangeGridPosition(int change)
    {
        int newPosition = currentPosition + change;
        int optionCount = options.Length;

        if (currentPosition >= 3 && currentPosition <= 5 && change == gridColumns)
        {
            newPosition = 6;
        }
        else if (currentPosition == 6)
        {
            if (change == -gridColumns)
            {
                newPosition = 4;
            }
            else
            {
                return;
            }
        }
        else
        {
            bool isHorizontal = Mathf.Abs(change) == 1;
            if (isHorizontal)
            {
                int currentRow = currentPosition / gridColumns;
                int newRow = newPosition / gridColumns;

                if (newPosition < 0 || newPosition >= optionCount || newRow != currentRow)
                    return;
            }
            else
            {
                if (newPosition < 0 || newPosition >= optionCount)
                    return;
            }
        }

        currentPosition = newPosition;

        if (change != 0)
            SoundManager.instance.PlaySound(changeSound);

        if (!useGridNavigation)
        {
            rect.position = new Vector3(options[currentPosition].position.x - 250, options[currentPosition].position.y, 0);
        }
        else
        {
            if (skillUI != null && skillUI.activeInHierarchy)
            {
                rect.position = new Vector3(options[currentPosition].position.x - 80, options[currentPosition].position.y, 0);
                UpdateSkillInfo();
            }
            else
                rect.position = new Vector3(options[currentPosition].position.x - 150, options[currentPosition].position.y, 0);
        }
    }

    private void Interact()
    {
        if (options[currentPosition].GetComponent<Button>() != null && options[currentPosition].GetComponent<Button>().interactable)
        {
            SoundManager.instance.PlaySound(interactSound);
            options[currentPosition].GetComponent<Button>().onClick.Invoke();
        }
    }

    public void ResetArrowPosition()
    {
        currentPosition = 0;

        if (options.Length > 0 && options[0] != null)
        {
            float offset = useGridNavigation ? 150 : 250;
            rect.position = new Vector3(options[0].position.x - offset, options[0].position.y, 0);
        }
    }

    public void UpdateSkillInfo()
    {
        if (closeButton != null && skillUI.activeInHierarchy && currentPosition == 2)
        {
            skillNameText.text = "";
            skillDescriptionText.text = "";
            rect.position = new Vector3(options[currentPosition].position.x - 220, options[currentPosition].position.y, 0);
        }
        else if (currentPosition >= 0 && currentPosition < skills.Length)
        {
            skillNameText.text = skills[currentPosition].name;
            skillDescriptionText.text = skills[currentPosition].description;
        }
    }
}