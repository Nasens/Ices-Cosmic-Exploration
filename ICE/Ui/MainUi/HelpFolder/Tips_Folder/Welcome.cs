using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class Welcome
    {
        public static void Draw()
        {
            // ImGui.TextWrapped($"Welcome! This is probably the most complicated plugin I've created so far.");
            ImGui.TextWrapped($"欢迎！这可能是目前我做过最复杂的插件。");
            // ImGui.TextWrapped($"This plugin is designed for specifically for the use of Cosmic Exploration, and is kinda hefty. So I'm going to try and go through all the different tips / tricks");
            ImGui.TextWrapped($"本插件专为宇宙探索设计，功能较多。我会尽量介绍各种使用技巧。");

            ImGui.Dummy(new(0, 5));

            // ImGui.TextWrapped("To the side you'll find a couple of different tabs that will *try* and answer any question that you migth have.");
            ImGui.TextWrapped("左侧有几个不同的标签页，会*尽量*回答你可能有的任何问题。");
            // ImGui.TextWrapped("PLEASE MAKE SURE TO CHECK THE REQUIREMENTS SECTION TO SEE WHAT YOU NEED FOR WHAT");
            ImGui.TextWrapped("请务必查看「插件依赖」部分，了解各项功能需要什么插件");
            // ImGui.TextWrapped("Or just read a specific tab to find out. Probably would answer a lot of questions");
            ImGui.TextWrapped("或者直接阅读对应标签页，应该能解答大部分疑问");
        }
    }
}
