using System;
using System.Collections.Generic;
using System.Text;

namespace ICE.Ui.MainUi.HelpFolder.Tips_Folder
{
    internal class Welcome
    {
        public static void Draw()
        {
            ImGui.TextWrapped($"欢迎！这大概是我目前为止做过的最复杂的插件。");
            ImGui.TextWrapped($"这个插件是专门为 Cosmic Exploration 设计的，内容相当庞杂。所以我会尽量把各种提示/技巧都讲一遍");

            ImGui.Dummy(new(0, 5));

            ImGui.TextWrapped("在侧边栏你会看到几个不同的标签页，它们会*尝试*解答你可能有的任何问题。");
            ImGui.TextWrapped("请务必查看依赖项部分，了解哪些功能需要哪些插件");
            ImGui.TextWrapped("或者直接阅读某个具体的标签页来了解。这大概能解答很多问题");
        }
    }
}
