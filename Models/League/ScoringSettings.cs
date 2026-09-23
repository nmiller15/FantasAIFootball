using System.Text.Json.Serialization;

namespace FantasAIFootball.Models.League;

public class ScoringSettings
{
    [JsonPropertyName("sack")]
    public decimal Sack { get; set; }
    [JsonPropertyName("fgm_40_49")]
    public decimal FieldGoalMade40to49 { get; set; }
    [JsonPropertyName("pass_int")]
    public decimal PassInterception { get; set; }
    [JsonPropertyName("pts_allow_0")]
    public decimal PointsAllowed0 { get; set; }
    [JsonPropertyName("pass_2pt")]
    public decimal PassTwoPointConversion { get; set; }
    [JsonPropertyName("st_td")]
    public decimal SpecialTeamsTouchdown { get; set; }
    [JsonPropertyName("rec_td")]
    public decimal ReceptionTouchdown { get; set; }
    [JsonPropertyName("fgm_30_39")]
    public decimal FieldGoalMade30to39 { get; set; }
    [JsonPropertyName("fgm_50_59")]
    public decimal FieldGoalMade50to59 { get; set; }
    [JsonPropertyName("xpmiss")]
    public decimal ExtraPointMissed { get; set; }
    [JsonPropertyName("rush_td")]
    public decimal RushingTouchdown { get; set; }
    [JsonPropertyName("pass_td_40p")]
    public decimal PassTouchdown40Plus { get; set; }
    [JsonPropertyName("rec_2pt")]
    public decimal ReceptionTwoPointConversion { get; set; }
    [JsonPropertyName("pass_int_td")]
    public decimal PassInterceptionTouchdown { get; set; }
    [JsonPropertyName("st_fum_rec")]
    public decimal SpecialTeamsFumbleRecovery { get; set; }
    [JsonPropertyName("fgmiss")]
    public decimal FieldGoalMissed { get; set; }
    [JsonPropertyName("ff")]
    public decimal ForcedFumble { get; set; }
    [JsonPropertyName("rec")]
    public decimal Reception { get; set; }
    [JsonPropertyName("pts_allow_14_20")]
    public decimal PointsAllowed14to20 { get; set; }
    [JsonPropertyName("fgm_0_19")]
    public decimal FieldGoalMade0to19 { get; set; }
    [JsonPropertyName("int")]
    public decimal Interception { get; set; }
    [JsonPropertyName("def_st_fum_rec")]
    public decimal DefensiveSpecialTeamsFumbleRecovery { get; set; }
    [JsonPropertyName("fum_lost")]
    public decimal FumbleLost { get; set; }
    [JsonPropertyName("pts_allow_1_6")]
    public decimal PointsAllowed1to6 { get; set; }
    [JsonPropertyName("fgm_60p")]
    public decimal FieldGoalMade60Plus { get; set; }
    [JsonPropertyName("fgm_20_29")]
    public decimal FieldGoalMade20to29 { get; set; }
    [JsonPropertyName("pts_allow_21_27")]
    public decimal PointsAllowed21to27 { get; set; }
    [JsonPropertyName("xpm")]
    public decimal ExtraPointMade { get; set; }
    [JsonPropertyName("pass_td_50p")]
    public decimal PassTouchdown50Plus { get; set; }
    [JsonPropertyName("rush_2pt")]
    public decimal RushingTwoPointConversion { get; set; }
    [JsonPropertyName("fum_rec")]
    public decimal FumbleRecovery { get; set; }
    [JsonPropertyName("def_st_td")]
    public decimal DefensiveSpecialTeamsTouchdown { get; set; }
    [JsonPropertyName("pass_cmp_40p")]
    public decimal PassCompletion40Plus { get; set; }
    [JsonPropertyName("def_td")]
    public decimal DefensiveTouchdown { get; set; }
    [JsonPropertyName("safe")]
    public decimal Safety { get; set; }
    [JsonPropertyName("pass_yd")]
    public decimal PassingYard { get; set; }
    [JsonPropertyName("blk_kick")]
    public decimal BlockedKick { get; set; }
    [JsonPropertyName("pass_td")]
    public decimal PassingTouchdown { get; set; }
    [JsonPropertyName("rush_yd")]
    public decimal RushingYard { get; set; }
    [JsonPropertyName("fum")]
    public decimal Fumble { get; set; }
    [JsonPropertyName("pts_allow_28_34")]
    public decimal PointsAllowed28to34 { get; set; }
    [JsonPropertyName("pts_allow_35p")]
    public decimal PointsAllowed35Plus { get; set; }
    [JsonPropertyName("fum_rec_td")]
    public decimal FumbleRecoveryTouchdown { get; set; }
    [JsonPropertyName("rec_yd")]
    public decimal ReceptionYard { get; set; }
    [JsonPropertyName("def_st_ff")]
    public decimal DefensiveSpecialTeamsForcedFumble { get; set; }
    [JsonPropertyName("pts_allow_7_13")]
    public decimal PointsAllowed7to13 { get; set; }
    [JsonPropertyName("st_ff")]
    public decimal SpecialTeamsForcedFumble { get; set; }
}
