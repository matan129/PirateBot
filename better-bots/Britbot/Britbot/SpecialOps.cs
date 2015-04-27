using System.Collections.Generic;
using System.Linq;
using Pirates;

namespace Britbot
{
    /// <summary>
    ///     Manages special game operations (e.x Cloak)
    /// </summary>
    internal static class SpecialOps
    {
        /// <summary>
        ///     Reveals cloaked pirates when needed and cloaks uncloaked ones when needed
        /// </summary>
        public static void DoCloak(Dictionary<Pirate,Direction> moves)
        {
            Group g = null;

            //the group that contains the cloaked pirate if one exists
            if(Bot.Game.GetMyCloaked() != null)
                //sorry for this horrible lambda, stuff went quite complex and I didn't have the time 
                // to restore the original function. This lambda finds the group with the cloaked pirate
                g = Commander.Groups.First(commGroup => commGroup.Pirates.ToList()
                    .ConvertAll(p => Bot.Game.GetMyPirate(p))
                    .Any(pirate => Bot.Game.GetMyCloaked().Id == pirate.Id));

            // if a pirate is cloaked and close enough to its target, reveal it
            if ((g != null) && (g.DistanceFromTarget <= Magic.CloakRange))
                moves[Bot.Game.GetMyCloaked()] = Direction.REVEAL;

            // if no pirate is cloaked and we can cloak one
            if (Bot.Game.CanCloak())
            {
                //All the single pirate groups that can be cloaked
                IEnumerable<Group> ones = Commander.Groups.Where(p => p.Pirates.Count == 1);

                //if there are any 1 pirate groups
                if (ones.Count() != 0)
                {
                    //the minimum distance from a target of one of the groups
                    int minDistance = ones.Min(group => @group.DistanceFromTarget);

                    //finds the group that the minimal distance belongs to and cloaks it
                    foreach (Group tc in ones)
                    {
                        if ((tc.DistanceFromTarget == minDistance)&&(minDistance>Magic.CloakRange))
                            moves[Bot.Game.GetMyPirate(tc.Pirates.First())] = Direction.CLOAK;
                    }
                }
            }
        }
    }
}