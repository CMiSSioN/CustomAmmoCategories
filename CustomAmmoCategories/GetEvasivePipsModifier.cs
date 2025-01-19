﻿/*  
 *  This file is part of CustomAmmoCategories.
 *  CustomAmmoCategories is free software: you can redistribute it and/or modify it under the terms of the 
 *  GNU Lesser General Public License as published by the Free Software Foundation, either version 3 of the License, 
 *  or (at your option) any later version. CustomAmmoCategories is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
 *  See the GNU Lesser General Public License for more details.
 *  You should have received a copy of the GNU Lesser General Public License along with CustomAmmoCategories. 
 *  If not, see <https://www.gnu.org/licenses/>. 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using BattleTech;
using BattleTech.AttackDirectorHelpers;
using System.Reflection;
using CustAmmoCategories;
using UnityEngine;
using CustomAmmoCategoriesLog;
using CustomAmmoCategoriesPatches;

namespace CustAmmoCategories {
  public static partial class CustomAmmoCategories {
    public static float EvasivePipsIgnored(this Weapon weapon) {
      Statistic stat = weapon.StatCollection.GetStatistic(Weapon_InitStats.EvasivePipsIgnoredStatName);
      if(stat != null) {
        return stat.Value<float>() + weapon.ammo().EvasivePipsIgnored + weapon.mode().EvasivePipsIgnored;
      } else {
        return weapon.weaponDef.EvasivePipsIgnored + weapon.ammo().EvasivePipsIgnored + weapon.mode().EvasivePipsIgnored;
      }
    }
  }
}

namespace CustomAmmoCategoriesPatches {
  [HarmonyPatch(typeof(ToHit))]
  [HarmonyPatch("GetEvasivePipsModifier")]
  [HarmonyPatch(MethodType.Normal)]
  [HarmonyPatch(new Type[] { typeof(int), typeof(Weapon) })]
  public static class ToHit_GetEvasivePipsModifier {
    public static void Prefix(ref bool __runOriginal,ToHit __instance, int evasivePips, Weapon weapon, ref float __result) {
      //CustomAmmoCategoriesLog.Log.LogWrite("ToHit.GetEvasivePipsModifier");
      if (!__runOriginal) { return; }
      try {
        float num = 0.0f;
        CombatGameState combat = __instance.combat;
        if (evasivePips > 0) {
          int index = Mathf.RoundToInt((float)((double)evasivePips - 1.0 - (weapon == null ? 0.0 : (double)(weapon.EvasivePipsIgnored()))));
          if (index >= combat.Constants.ToHit.ToHitMovingPipUMs.Length) { index = combat.Constants.ToHit.ToHitMovingPipUMs.Length - 1; };
          if (index >= 0) {
            num += combat.Constants.ToHit.ToHitMovingPipUMs[index];
          }
        }
        __result = num;
        __runOriginal = false;
        return;
      } catch (Exception e) {
        Log.Combat?.WL(0,"Exception " + e.ToString() + "\nFallback to default");
        ToHit.hitLogger.LogException(e);
        return;
      }
    }
  }
  public static class Weapon_InitStats {
    public static readonly string EvasivePipsIgnoredStatName = "EvasivePipsIgnored";
    public static void Postfix(Weapon __instance) {
      try {
        __instance.StatCollection.AddStatistic<float>(EvasivePipsIgnoredStatName, __instance.weaponDef.EvasivePipsIgnored);
      } catch (Exception e) {
        Log.Combat?.WL(0,e.ToString(),true);
        Weapon.logger.LogException(e);
      }
    }
  }
}
