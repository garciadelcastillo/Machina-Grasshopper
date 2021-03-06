﻿using Machina;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper.Kernel;

namespace MachinaGrasshopper.Program
{
    //   ██████╗ ██████╗ ███╗   ███╗██████╗ ██╗██╗     ███████╗██████╗ ██████╗  ██████╗  ██████╗ ██████╗  █████╗ ███╗   ███╗
    //  ██╔════╝██╔═══██╗████╗ ████║██╔══██╗██║██║     ██╔════╝██╔══██╗██╔══██╗██╔═══██╗██╔════╝ ██╔══██╗██╔══██╗████╗ ████║
    //  ██║     ██║   ██║██╔████╔██║██████╔╝██║██║     █████╗  ██████╔╝██████╔╝██║   ██║██║  ███╗██████╔╝███████║██╔████╔██║
    //  ██║     ██║   ██║██║╚██╔╝██║██╔═══╝ ██║██║     ██╔══╝  ██╔═══╝ ██╔══██╗██║   ██║██║   ██║██╔══██╗██╔══██║██║╚██╔╝██║
    //  ╚██████╗╚██████╔╝██║ ╚═╝ ██║██║     ██║███████╗███████╗██║     ██║  ██║╚██████╔╝╚██████╔╝██║  ██║██║  ██║██║ ╚═╝ ██║
    //   ╚═════╝ ╚═════╝ ╚═╝     ╚═╝╚═╝     ╚═╝╚══════╝╚══════╝╚═╝     ╚═╝  ╚═╝ ╚═════╝  ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚═╝
    //                                                                                                                      
    public class Compile : GH_Component
    {
        public Compile() : base(
            "CompileProgram",
            "CompileProgram",
            "Compiles a list of Actions into the device's native language. This is the code you would typically need to load into the device's controller to run the program.",
            "Machina",
            "Program")
        { }
        public override GH_Exposure Exposure => GH_Exposure.primary;
        public override Guid ComponentGuid => new Guid("8355fbed-7aa0-4a29-bd9a-c8dca15f2bfb");
        protected override System.Drawing.Bitmap Icon => Properties.Resources.Program_Compile;

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Robot", "R", "The Robot instance that will export this program", GH_ParamAccess.item);
            pManager.AddGenericParameter("Actions", "A", "A program in the form of a list of Actions", GH_ParamAccess.list);
            pManager.AddBooleanParameter("InLineTargets", "i", "If true, targets will be declared inline with the instruction. Otherwise, the will be declared and used as independent variables", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Comments", "c", "If true, Machina-style comments with code information will be added to the end of the code instructions", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            //pManager.AddTextParameter("Code", "C", "Device-specific program code", GH_ParamAccess.item);
            pManager.AddGenericParameter("RobotProgram", "P", "Device-specific program", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Machina.Robot bot = null;
            List<Machina.Action> actions = new List<Machina.Action>();
            bool inline = false;
            bool comments = false;

            if (!DA.GetData(0, ref bot)) return;
            if (!DA.GetDataList(1, actions)) return;
            if (!DA.GetData(2, ref inline)) return;
            if (!DA.GetData(3, ref comments)) return;

            // Sanity, avoid users compiling programs with inadvertedly null actions.
            foreach (var a in actions)
            {
                if (a == null)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Can't compile a Program with `null` Actions, please review the Action list.");
                    return;
                }
            }

            // Create a new instance to avoid inheriting robot states between different compilations
            // https://github.com/RobotExMachina/Machina-Grasshopper/issues/3
            Machina.Robot compiler = Machina.Robot.Create(bot.Name, bot.Brand);

            compiler.ControlMode(ControlType.Offline);
            foreach (Machina.Action a in actions)
            {
                compiler.Issue(a);
            }

            Machina.Types.Data.RobotProgram program = compiler.Compile(inline, comments);
                        
            DA.SetData(0, program);
        }
    }
}
