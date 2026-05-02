using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Luna;
using CupkekGames.InventorySystem;
using System.Threading;
using Cysharp.Threading.Tasks;
using CupkekGames.TimeSystem;
using CupkekGames.RPGStats;

namespace CupkekGames.Combat
{
  [System.Serializable]
  public class CombatAttributeDataEffectRuntime
  {
    public AttributeEffect Data;
    public Guid ID;
    public float StartDuration;
    private CountdownTimeContext _countdown;
    public CountdownTimeContext Countdown => _countdown;
    public float Duration
    {
      get
      {
        return _countdown.Value;
      }
      set
      {
        if (value > StartDuration)
        {
          StartDuration = value;
        }

        _countdown.Value = value;
      }
    }
    public Sprite Icon;
    public int Level;
    public string Name;
    public string Description;
    public UIColor Color;
    public bool ImageTint = false;
    public Func<List<ItemStatDisplayLine>> ExtraLinesLayerOne;
    public Func<List<ItemStatDisplayLine>> ExtraLinesLayerTwo;
    public bool Show = true;
    private CombatUnit _caster;
    public event Action<CombatAttributeDataEffectRuntime> OnEnd;
    public CombatAttributeDataEffectRuntime(Guid? id, AttributeEffect data, float duration, int level, Sprite icon, string name,
      string description, UIColor color, bool imageTint, Func<List<ItemStatDisplayLine>> extraLinesLayerOne = null,
      Func<List<ItemStatDisplayLine>> extraLinesLayerTwo = null, bool show = true)
    {
      if (id.HasValue)
      {
        ID = id.Value;
      }
      else
      {
        ID = Guid.NewGuid();
      }

      Data = data;
      StartDuration = duration;
      _countdown = new CountdownTimeContext(TimeManager.Instance.Global, StartDuration, 0, StatusEffect.INTERVAL_VISUAL, CancellationToken.None);
      Level = level;
      Icon = icon;
      Name = name;
      Description = description;
      Color = color;
      ImageTint = imageTint;
      ExtraLinesLayerOne = extraLinesLayerOne;
      ExtraLinesLayerTwo = extraLinesLayerTwo;
      Show = show;
    }

    public virtual List<ItemStatDisplayLine> GetTooltipLines()
    {
      List<ItemStatDisplayLine> lines = new();
      if (ExtraLinesLayerOne != null)
      {
        lines.AddRange(ExtraLinesLayerOne());
      }
      if (ExtraLinesLayerTwo != null)
      {
        lines.AddRange(ExtraLinesLayerTwo());
      }

      foreach (string line in Data.GetStringEffectList())
      {
        lines.Add(new ItemStatDisplayLine { Value = line });
      }

      return lines;
    }

    public void StartCooldown(CombatUnit caster)
    {
      _countdown.Dispose();

      _countdown = new CountdownTimeContext(caster.TimeBundle.TimeContext, StartDuration, 0, StatusEffect.INTERVAL_VISUAL, caster.DeathToken.Token);
      _countdown.OnComplete += OnComplete;
      _countdown.Start();
    }

    private void OnComplete()
    {
      OnEnd?.Invoke(this);

      _countdown.Dispose();
    }
    public void StopCooldown()
    {
      _countdown.Dispose();
    }
  }
}