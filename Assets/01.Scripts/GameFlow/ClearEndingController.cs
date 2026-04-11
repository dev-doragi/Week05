// using System.Collections;
// using System.Collections.Generic;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;

// public class ClearEndingController : MonoBehaviour
// {
//     [SerializeField] private GameObject endingPannel;
//     [SerializeField] private Slider progressSlider;
//     [SerializeField] private TextMeshProUGUI[] messageSlots;
//     [SerializeField] private float totalSeconds = 8f;

//     private bool _running;

//     public void PlayClearSequence(IReadOnlyList<IssueDefinition> issues)
//     {
//         if (_running) return;
//         StartCoroutine(CoPlay(issues));
//     }

//     private IEnumerator CoPlay(IReadOnlyList<IssueDefinition> issues)
//     {
//         _running = true;

//         UIManager.Instance?.EditorPannelPopup();
//         if (endingPannel != null) endingPannel.SetActive(true);

//         ResetUI();

//         int count = Mathf.Min(issues != null ? issues.Count : 0, messageSlots != null ? messageSlots.Length : 0);
//         float elapsed = 0f;
//         int next = 0;
//         float per = count > 0 ? totalSeconds / count : totalSeconds;

//         while (elapsed < totalSeconds)
//         {
//             elapsed += Time.deltaTime;
//             if (progressSlider != null) progressSlider.value = Mathf.Clamp01(elapsed / totalSeconds);

//             while (next < count && elapsed >= per * (next + 1))
//             {
//                 Show(next, issues[next]);
//                 next++;
//             }

//             yield return null;
//         }

//         if (progressSlider != null) progressSlider.value = 1f;
//         while (next < count) { Show(next, issues[next]); next++; }

//         _running = false;
//     }

//     private void ResetUI()
//     {
//         if (progressSlider != null) progressSlider.value = 0f;
//         if (messageSlots == null) return;

//         for (int i = 0; i < messageSlots.Length; i++)
//         {
//             if (messageSlots[i] == null) continue;
//             messageSlots[i].text = string.Empty;

//             Transform parent = messageSlots[i].transform.parent;
//             if (parent != null) parent.gameObject.SetActive(false);
//         }
//     }

//     private void Show(int index, IssueDefinition issue)
//     {
//         if (messageSlots == null || index < 0 || index >= messageSlots.Length) return;
//         if (messageSlots[index] == null || issue == null) return;

//         Transform parent = messageSlots[index].transform.parent;
//         if (parent != null) parent.gameObject.SetActive(true);

//         messageSlots[index].text = $"[{issue.IssueId}] {issue.IssueTitle}";
//     }
// }
