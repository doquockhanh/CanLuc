# Hệ thống Kill Sound

## Tổng quan
Hệ thống kill sound cho phép phát âm thanh khác nhau dựa trên số kill của player, với 12 kill sounds tăng dần và logic đặc biệt khi tất cả enemy bị tiêu diệt.

## Các thành phần chính

### 1. KillSoundManager
- **Vị trí**: `Assets/Scripts/Audio/KillSoundManager.cs`
- **Chức năng**: Quản lý 12 kill sounds và âm thanh chiến thắng
- **Singleton**: Có thể truy cập qua `KillSoundManager.Instance`

#### Cấu hình trong Inspector:
- `AudioSource`: AudioSource để phát âm thanh
- `Kill Sounds`: Mảng 12 AudioClip cho kill sounds (1-12)
- `Victory Music`: Nhạc nền khi tất cả enemy bị tiêu diệt
- `Victory Effect`: Hiệu ứng ăn mừng
- `Victory Music Volume`: Âm lượng nhạc nền (0-1)
- `Victory Effect Volume`: Âm lượng hiệu ứng (0-1)

#### API chính:
```csharp
// Phát kill sound dựa trên số kill
KillSoundManager.Instance.PlayKillSound(int killCount);

// Phát âm thanh chiến thắng
KillSoundManager.Instance.PlayVictorySounds();

// Dừng nhạc nền chiến thắng
KillSoundManager.Instance.StopVictoryMusic();

// Kiểm tra đang phát nhạc nền chiến thắng
bool isPlaying = KillSoundManager.Instance.IsPlayingVictoryMusic();
```

### 2. ScoreManager (Cập nhật)
- **Vị trí**: `Assets/Scripts/GameManager/ScoreManager.cs`
- **Chức năng mới**: Theo dõi kill count và tự động phát kill sound

#### API mới:
```csharp
// Tăng kill count và phát kill sound
ScoreManager.Instance.AddKill();

// Reset kill count về 0
ScoreManager.Instance.ResetKillCount();

// Lấy kill count hiện tại
int kills = ScoreManager.Instance.GetKillCount();

// Thiết lập kill count
ScoreManager.Instance.SetKillCount(int newKillCount);
```

#### Events mới:
```csharp
// Event khi kill count thay đổi
ScoreManager.Instance.OnKillCountChanged += (int killCount) => {
    Debug.Log($"Kill count: {killCount}");
};
```

### 3. EnemyStats (Cập nhật)
- **Vị trí**: `Assets/Scripts/Enemies/EnemyStats.cs`
- **Chức năng mới**: Tự động tăng kill count khi bị player tiêu diệt

#### Logic:
- Khi enemy bị player tiêu diệt: Cộng điểm + Tăng kill count + Phát kill sound
- Khi enemy bị FinishObstacle tiêu diệt: Chỉ phá hủy, không cộng điểm/kill

### 4. GameManager (Cập nhật)
- **Vị trí**: `Assets/Scripts/GameManager/GameManager.cs`
- **Chức năng mới**: Reset kill count khi bắt đầu battle phase và phát âm thanh chiến thắng

## Cách sử dụng

### 1. Thiết lập trong Unity Editor

1. **Tạo KillSoundManager**:
   - Tạo GameObject mới trong scene
   - Thêm component `KillSoundManager`
   - Gán AudioSource và 12 kill sound clips
   - Gán victory music và victory effect

2. **Cấu hình ScoreManager**:
   - ScoreManager đã có sẵn, không cần thay đổi gì
   - Kill count sẽ tự động reset khi bắt đầu battle phase

3. **Kiểm tra EnemyStats**:
   - Tất cả enemy có component `EnemyStats` sẽ tự động hoạt động
   - Không cần cấu hình thêm

### 2. Logic hoạt động

1. **Khi enemy bị player tiêu diệt**:
   ```
   EnemyStats.DestroyEnemy() 
   → AwardScore() (cộng điểm)
   → AwardKill() (tăng kill count)
   → ScoreManager.AddKill()
   → KillSoundManager.PlayKillSound(killCount)
   ```

2. **Khi enemy bị FinishObstacle tiêu diệt**:
   ```
   EnemyStats.DestroyEnemy()
   → Chỉ phá hủy, không cộng điểm/kill
   ```

3. **Khi tất cả enemy bị tiêu diệt**:
   ```
   GameManager.CheckAllEnemiesCleared()
   → PlayVictorySounds()
   → KillSoundManager.PlayVictorySounds()
   → Phát victory effect + victory music
   ```

### 3. Kill Sound Mapping

| Kill Count | Sound Index | Mô tả |
|------------|-------------|-------|
| 1-12       | 0-11        | Kill sounds tăng dần |
| 13+        | 11          | Sử dụng kill sound 12 |

### 4. Debug & Testing

#### Context Menu trong Inspector:
- **ScoreManager**: "Add Kill", "Reset Kill Count", "Set Kill Count to X"
- **KillSoundManager**: "Test Kill Sound X", "Test Victory Sounds"

#### Console Logs:
- Tất cả hoạt động đều có log để debug
- Có thể bật/tắt logging trong Inspector

## Lưu ý quan trọng

1. **Thứ tự khởi tạo**: KillSoundManager và ScoreManager phải được khởi tạo trước khi enemy bị tiêu diệt
2. **AudioSource**: KillSoundManager cần AudioSource để phát âm thanh
3. **Kill Sounds**: Cần gán đủ 12 AudioClip trong Inspector
4. **Performance**: Kill sound được phát qua `PlayOneShot()` nên không ảnh hưởng đến âm thanh khác
5. **Memory**: Victory music được phát loop, nhớ dừng khi cần thiết

## Troubleshooting

### Kill sound không phát:
- Kiểm tra KillSoundManager.Instance có null không
- Kiểm tra AudioSource đã được gán chưa
- Kiểm tra kill sounds array có đủ 12 clips không

### Victory sound không phát:
- Kiểm tra victory music và victory effect đã được gán chưa
- Kiểm tra GameManager có gọi PlayVictorySounds() không

### Kill count không tăng:
- Kiểm tra enemy có bị player tiêu diệt không (không phải FinishObstacle)
- Kiểm tra ScoreManager.Instance có null không
- Kiểm tra EnemyStats.AwardKill() có được gọi không
