using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class HybridUpgradeRepository : IUpgradeRepository
{
    private readonly IUpgradeRepository _localRepo;
    private readonly IUpgradeRepository _firebaseRepo;

    private CancellationTokenSource _debounceCts;
    private const float DebounceDelaySeconds = 0.6f;

    public HybridUpgradeRepository(string userId)
    {
        _localRepo = new LocalUpgradeRepository(userId);
        _firebaseRepo = new FirebaseUpgradeRepository(userId);
    }

    public UniTask Save(UpgradeSaveData saveData)
    {
        // 기존 debounce 타이머 취소
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();
        _debounceCts = new CancellationTokenSource();

        SaveDebounced(saveData, _debounceCts.Token).Forget();
        return UniTask.CompletedTask;
    }

    private async UniTaskVoid SaveDebounced(UpgradeSaveData saveData, CancellationToken ct)
    {
        try
        {
            await UniTask.Delay((int)(DebounceDelaySeconds * 1000), cancellationToken: ct);

            await _localRepo.Save(saveData);
            await _firebaseRepo.Save(saveData);
            Debug.Log("[HybridUpgradeRepository] 저장 완료");
        }
        catch (System.OperationCanceledException)
        {
            // debounce로 인해 취소됨 - 정상 동작
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[HybridUpgradeRepository] 저장 실패: {e.Message}");
        }
    }

    public async UniTask<UpgradeSaveData> Load()
    {
        // 로컬과 Firebase에서 병렬 로드
        var (localData, firebaseData) = await UniTask.WhenAll(
            _localRepo.Load(),
            _firebaseRepo.Load()
        );

        // 타임스탬프 비교하여 최신 데이터 선택
        var newer = SaveDataBase.GetNewer(localData, firebaseData);

        // Firebase 데이터가 더 최신이면 로컬에도 저장
        if (newer == firebaseData && firebaseData != null)
        {
            Debug.Log("[HybridUpgradeRepository] Firebase 데이터가 더 최신입니다. 로컬에도 저장합니다.");
            await _localRepo.Save(firebaseData);
        }
        // 로컬 데이터가 더 최신이면 Firebase에도 저장
        else if (newer == localData && localData != null && firebaseData != null)
        {
            Debug.Log("[HybridUpgradeRepository] 로컬 데이터가 더 최신입니다. Firebase에도 저장합니다.");
            _firebaseRepo.Save(localData).Forget();
        }

        return newer ?? UpgradeSaveData.Default;
    }
}
