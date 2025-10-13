/// <summary>
/// 중력 시뮬레이션을 위한 블록 이동 로직을 처리하는 클래스
/// 순수한 그리드 상태 변경을 담당하며, 애니메이션과는 분리되어 있습니다.
/// </summary>
public class BlockMover
{
    public void MoveBlocksDown(GridManager gridmanager)
    {
        // null GridManager 방어 코드
        if (gridmanager == null)
        {
            return;
        }

        bool hasmoved = true;

        // 더 이상 이동할 블록이 없을 때까지 반복
        while (hasmoved)
        {
            hasmoved = false;

            // 아래에서 위로 스캔하면서 각 블록을 한 칸씩만 이동
            for (int x = 0; x < gridmanager.Width; x++)
            {
                for (int y = gridmanager.Height - 2; y >= 0; y--)
                {
                    var currentkey = (x, y);
                    var block = gridmanager.GetBlock(currentkey);

                    // 블록이 있는 경우
                    if (block != null)
                    {
                        // 바로 아래 슬롯 확인
                        var belowkey = (x, y + 1);
                        var belowblock = gridmanager.GetBlock(belowkey);

                        // 아래가 비어있으면 한 칸 이동
                        if (belowblock == null)
                        {
                            gridmanager.SetBlock(belowkey, block);
                            gridmanager.SetBlock(currentkey, null);
                            hasmoved = true;
                        }
                    }
                }
            }
        }
    }
}
