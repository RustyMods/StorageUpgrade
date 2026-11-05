using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StorageUpgrade;

public class InvButton
{
    public GameObject instance;
    public RectTransform rect;
    public Button button;
    public TextMeshProUGUI tmp;
    public UIGamePad gamepad;
    public UIInputHint gamepad_hint;
    public TextMeshProUGUI gamepad_tmp;
    public UITooltip tooltip;

    public InvButton(GameObject source, Transform parent, Vector2 position)
    {
        instance = Object.Instantiate(source, parent);
        rect = (instance.transform as RectTransform)!;
        button = instance.GetComponent<Button>();
        gamepad = instance.GetComponent<UIGamePad>();
        tmp = instance.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        gamepad_hint = instance.transform.Find("gamepad_hint").GetComponent<UIInputHint>();
        gamepad_tmp = gamepad_hint.m_gamepadHint.GetComponent<TextMeshProUGUI>();
        tooltip = instance.AddComponent<UITooltip>();
        rect.anchoredPosition = position;
    }
    
    public void SetLabel(string label) => tmp.text = Localization.instance.Localize(label);

    public void SetGamepadKey(string key)
    {
        gamepad.m_zinputKey = key;
        gamepad_tmp.text = Localization.instance.Localize(
            ZInput.instance.GetBoundKeyString(key, true));
    }
    
    public void SetAnchoredPosition(Vector2 pos) => rect.anchoredPosition = pos;
}