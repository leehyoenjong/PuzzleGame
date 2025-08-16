using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolSystem_Block : MonoBehaviour
{
    [SerializeField] List<St_BlockData> _blocklist;
    Dictionary<EMATCHTYPE, Queue<UI_Match_Block>> _blockpooldic = new Dictionary<EMATCHTYPE, Queue<UI_Match_Block>>();

    void Awake()
    {
        SettingBlock();
    }

    void OnEnable()
    {
        MatchFiledManager._block_create_event += CreateBlock;
        UI_Match_Block._mathcomplte_event += Release;
    }

    void OnDisable()
    {
        MatchFiledManager._block_create_event -= CreateBlock;
        UI_Match_Block._mathcomplte_event -= Release;
    }

    void SettingBlock()
    {
        foreach (var item in _blocklist)
        {
            _blockpooldic.Add(item._blocktypes, new Queue<UI_Match_Block>());
        }
    }

    UI_Match_Block CreateBlock(EMATCHTYPE blocktypes, Transform parent)
    {
        if (_blockpooldic.TryGetValue(blocktypes, out var blockpool) == false)
        {
            Debug.LogError("이상한 타입을 가져가려고 합니다.!");
            return default;
        }

        UI_Match_Block block = null;

        if (blockpool.Count > 0)
        {
            block = blockpool.Dequeue();
            return block;
        }

        var getblock = _blocklist.FirstOrDefault(x => x._blocktypes == blocktypes);
        if (getblock._blockobject == null)
        {
            Debug.LogError("가져오기 실패!");
            return default;
        }

        var createblock = Instantiate(getblock._blockobject, parent);
        var matchblock = createblock.GetComponent<UI_Match_Block>();
        matchblock.ResetPoint();
        matchblock.gameObject.SetActive(false);
        return matchblock;
    }

    void Release(UI_Match_Block relelseblock)
    {
        if (_blockpooldic.TryGetValue(relelseblock.GetBlockMatchTypes(), out var blockpool) == false)
        {
            Debug.LogError("타입이 없는 블록이 리턴됨");
            return;
        }

        relelseblock.DisableAni();
        relelseblock.ResetPoint();
        blockpool.Enqueue(relelseblock);
    }
}

[Serializable]
public struct St_BlockData
{
    public EMATCHTYPE _blocktypes;
    public GameObject _blockobject;
}