using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p3ppc.fesTactics;
internal class Tactics
{
    public static Tactic[] English =
    {
        new Tactic("Act Freely", "Teammate will decide", "his/her own actions."),
        new Tactic("Knock Down", "I don't know", "the description"),
        new Tactic("Attack Fallen", "Some description", "for attack fallen"),
        new Tactic("Full Assault", "Teammate will focus", "on attacking."),
        new Tactic("Heal/Support", "Teammate will concentrate", "on healing skills."),
        new Tactic("Conserve SP", "Teammate will avoid", "actions that use SP"),
        new Tactic("Same Target", "They target the same", "thing or something"),
        new Tactic("Assign Target", "Teammate will avoid", "actions that use SP"),
        new Tactic("Defend Self", "They don't die", "hopefully"),
        new Tactic("Run Away", "Teammate will", "escape battle."),
        new Tactic("Direct Commands", "You are the boss", "be nice :>"),
        new Tactic("Orgia Mode", "Aigis", "she angry D:"),
    };

}

internal class Tactic
{
    internal string Name { get; }

    // The first line of the description
    internal string Description1 { get; }
    // The second line of the description (this is how the game does it)
    internal string Description2 { get; }

    internal Tactic(string name, string description1, string description2)
    {
        Name = name;
        Description1 = description1;
        Description2 = description2;
    }
}
