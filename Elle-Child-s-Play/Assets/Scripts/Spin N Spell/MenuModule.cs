using UnityEngine;
using UnityEngine.EventSystems;

public class MenuModule : MonoBehaviour, ISelectHandler
{
    [HideInInspector]
    public int moduleIndex = -1;
    [HideInInspector]
    public GameMenu menu;
    public AudioSource aud;

    private static bool started = false;

    void Start()
    {
        for(int i = 0; i < transform.parent.childCount; i++)
        {
            if (transform.parent.GetChild(i) == transform)
                moduleIndex = i;
        }
    }

    void Update()
    {
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            Mathf.Lerp(transform.localPosition.z, (EventSystem.current.currentSelectedGameObject == gameObject ? -2 : 0), 3 * Time.deltaTime)
            );
    }

    public void StartModule()
    {
        menu.PickModule(moduleIndex);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (started == false)
            started = true;
        else
            aud.Play();
    }
}
