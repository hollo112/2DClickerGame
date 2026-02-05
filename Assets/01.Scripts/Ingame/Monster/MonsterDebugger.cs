// using UnityEngine;
// using System.Text;
//
// public class MonsterDebugger : MonoBehaviour
// {
//     void Update()
//     {
//         // 숫자 1키를 누르면 현재 상태 리포트 출력
//         if (Input.GetKeyDown(KeyCode.Alpha1))
//         {
//             PrintMonsterStatus();
//         }
//     }
//
//     private void PrintMonsterStatus()
//     {
//         if (MonsterInGameManager.Instance == null)
//         {
//             Debug.LogError("[Debugger] InGameMonsterManager 인스턴스를 찾을 수 없습니다.");
//             return;
//         }
//
//         var monsters = MonsterInGameManager.Instance.Monsters;
//         var data = MonsterInGameManager.Instance.Data;
//
//         StringBuilder sb = new StringBuilder();
//         sb.AppendLine("<b><color=cyan>===== [Monster Manager Status Report] =====</color></b>");
//         sb.AppendLine($"전체 리스트 개수: {monsters.Count}");
//
//         if (data != null)
//         {
//             sb.AppendLine($"설정된 MaxTier: {data.MaxTier}");
//             sb.AppendLine($"티어당 최대 마리수(MaxMonstersPerTier): {data.MaxMonstersPerTier}");
//             sb.AppendLine("--- 티어별 현황 ---");
//
//             for (int i = 0; i < data.MaxTier; i++)
//             {
//                 // 실제 리스트를 순회하며 해당 티어인 놈들을 직접 카운트
//                 int realCount = 0;
//                 foreach (var m in monsters)
//                 {
//                     if (m != null && m.Tier == i) realCount++;
//                 }
//
//                 sb.AppendLine($"[Tier {i}] {realCount} 마리 " +
//                               (realCount >= 3 ? "<color=green>(머지 가능 개수 충족)</color>" : "<color=red>(부족)</color>"));
//
//                 // 다음 단계 슬롯 확인
//                 if (i + 1 < data.Tiers.Length)
//                 {
//                     int nextTierCount = 0;
//                     foreach (var m in monsters)
//                         if (m != null && m.Tier == i + 1)
//                             nextTierCount++;
//
//                     if (nextTierCount >= data.MaxMonstersPerTier)
//                         sb.AppendLine(
//                             $"   ㄴ <color=yellow>주의: 다음 티어({i + 1})가 꽉 참 ({nextTierCount}/{data.MaxMonstersPerTier})</color>");
//                 }
//             }
//         }
//
//         sb.AppendLine("--- 개별 몬스터 상세 ---");
//         for (int i = 0; i < monsters.Count; i++)
//         {
//             var m = monsters[i];
//             if (m == null)
//                 sb.AppendLine($"[{i}] NULL (리스트 정리가 필요함)");
//             else
//                 sb.AppendLine($"[{i}] 이름: {m.name}, 티어: {m.Tier}, 위치: {m.transform.position}");
//
//             sb.AppendLine("<b><color=cyan>===========================================</color></b>");
//             Debug.Log(sb.ToString());
//         }
//     }
// }