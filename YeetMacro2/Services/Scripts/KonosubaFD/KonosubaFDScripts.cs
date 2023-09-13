using Microsoft.Extensions.Logging;
using YeetMacro2.Data.Models;
using YeetMacro2.ViewModels.NodeViewModels;

namespace YeetMacro2.Services.Scripts.KonosubaFD;

/* notepad++ replaces

,\s?({.*?(ClickPattern|DoClick|PredicatePattern).*?}) => , new PollPatternFindOptions\(\) $1

(\[(patterns.+?)\]) => new PatternNode[] { $2 }

patterns(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?

patterns(?{2}["$2"]:)(?{4}["$4"]:)(?{6}["$6"]:)(?{8}["$8"]:)(?{10}["$10"]:)(?{12}["$12"]:)(?{14}["$14"]:)(?{16}["$16"]:)(?{18}["$18"]:)(?{20}["$20"]:)

settings(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?(\.(\w+))?

settings(?{2}["$2"]:)(?{4}["$4"]:)(?{6}["$6"]:)(?{8}["$8"]:)(?{10}["$10"]:)(?{12}["$12"]:)(?{14}["$14"]:)(?{16}["$16"]:)(?{18}["$18"]:)(?{20}["$20"]:)

(const)|(let) => var

((ClickPattern)|(DoClick)|(PredicatePattern)).*?: => $1 =

' => "

logger.info => logger.LogInformation

logger.debug => logger.LogDebug

sleep\( => Thread.Sleep\(

=== => ==

\${ => {

`(.*?)` => $$"$1"

*/

public partial class KonosubaFDScripts
{
    ILogger logger;
    MacroService macroService;
    PatternNodeViewModel patterns;
    ParentSettingViewModel settings;

    public KonosubaFDScripts(ILogger logger, MacroService macroService, PatternNodeViewModel patterns, ParentSettingViewModel settings)
    {
        this.logger = logger;
        this.macroService = macroService;
        this.patterns = patterns;
        this.settings = settings;

        resolvePath(this.patterns);
    }

    public void resolvePath(PatternNode node, string path = "")
    {
        if (node.Name == "root")
        {
            node.Path = "";
        }
        else if (string.IsNullOrWhiteSpace(path))
        {
            node.Path = node.Name;
        }
        else
        {
            node.Path = $"{path}.{node.Name}";
        }

        foreach (var child in node.Nodes)
        {
            resolvePath(child, node.Path);
        }
    }
}