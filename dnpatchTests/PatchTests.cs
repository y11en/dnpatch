﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnpatch;
using dnpatch.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dnpatchTests
{
    [TestClass()]
    public class PatchTests
    {
        [TestMethod()]
        public void PatchDefault()
        {
            Loader loader = new Loader();

            loader.Initialize("crack", "Security.dll", "Security.default.dll", false, true, true); // crack the license mechanism
            loader.Initialize("credits", "UI.dll", "UI.default.dll", false, true, true); // add credits to the UI window

            Assembly security = loader.LoadAssembly("crack");
            Assembly ui = loader.LoadAssembly("credits");

            Console.WriteLine(security.AssemblyInfo.ToString());
            Console.WriteLine(ui.AssemblyInfo.ToString());

            security.Model.SetNamespace("Security");
            security.Model.SetType("Security");
            security.Model.SetMethod("IsLicensed");

            ui.Model.SetNamespace("UI");
            ui.Model.SetType("UI");
            ui.Model.SetMethod("GetCredits");

            security.IL.Overwrite(instructions: new Instruction[] // return true
            {
                Instruction.Create(OpCodes.Ldc_I4_1),
                Instruction.Create(OpCodes.Ret)
            });

            ui.IL.Write(Instruction.Create(OpCodes.Ldstr, "Cracked By Evil-Corp"), 1);

            loader.Save();

            var reflSecurity = ModuleDefMD.Load("Security.default.dll");
            var reflUi = ModuleDefMD.Load("UI.default.dll");

            var sol = new[]
            {
                OpCodes.Ldc_I4_1,
                OpCodes.Ret
            };
            for (var index = 0;
                index < reflSecurity.FindReflection("Security.Security").FindMethod("IsLicensed").Body.Instructions
                    .Count;
                index++)
            {
                var inst = reflSecurity.FindReflection("Security.Security").FindMethod("IsLicensed").Body
                    .Instructions[index];
                Assert.AreEqual(inst.OpCode, sol[index]);
            }

            Assert.AreEqual(reflUi.FindReflection("UI.UI").FindMethod("GetCredits").Body.Instructions[1].Operand, "Cracked By Evil-Corp");
        }

        [TestMethod()]
        public void PatchByReference()
        {
            Loader loader = new Loader();

            loader.Initialize("crack", "Security.dll", "Security.byref.dll", false, true, true); // crack the license mechanism
            loader.Initialize("credits", "UI.dll", "UI.byref.dll", false, true, true); // add credits to the UI window

            Assembly security = loader.LoadAssembly("crack");
            Assembly ui = loader.LoadAssembly("credits");

            Console.WriteLine(security.AssemblyInfo.ToString());
            Console.WriteLine(ui.AssemblyInfo.ToString());

            security.Model.SetNamespace("Security");
            security.Model.SetType(typeof(Security.Security));
            security.Model.SetMethod(typeof(Security.Security).GetMethod("IsLicensed"));

            ui.Model.SetNamespace("UI");
            ui.Model.SetType(typeof(UI.UI));
            ui.Model.SetMethod(typeof(UI.UI).GetMethod("GetCredits"));

            security.IL.Overwrite(instructions: new Instruction[] // return true
            {
                Instruction.Create(OpCodes.Ldc_I4_1),
                Instruction.Create(OpCodes.Ret)
            });

            ui.IL.Write(Instruction.Create(OpCodes.Ldstr, "Cracked By Evil-Corp"), 1);

            loader.Save();

            var reflSecurity = ModuleDefMD.Load("Security.byref.dll");
            var reflUi = ModuleDefMD.Load("UI.byref.dll");

            var sol = new[]
            {
                OpCodes.Ldc_I4_1,
                OpCodes.Ret
            };
            for (var index = 0;
                index < reflSecurity.FindReflection("Security.Security").FindMethod("IsLicensed").Body.Instructions
                    .Count;
                index++)
            {
                var inst = reflSecurity.FindReflection("Security.Security").FindMethod("IsLicensed").Body
                    .Instructions[index];
                Assert.AreEqual(inst.OpCode, sol[index]);
            }

            Assert.AreEqual(reflUi.FindReflection("UI.UI").FindMethod("GetCredits").Body.Instructions[1].Operand, "Cracked By Evil-Corp");
        }
    }
}