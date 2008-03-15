using System;

namespace libsecondlife
{
    /// <summary>
    /// Static pre-defined animations available to all agents
    /// </summary>
    public static class Animations
    {
        /// <summary>Agent with afraid expression on face</summary>
        public readonly static LLUUID AFRAID = new LLUUID("6b61c8e8-4747-0d75-12d7-e49ff207a4ca");
        /// <summary>Agent aiming a bazooka (right handed)</summary>
        public readonly static LLUUID AIM_BAZOOKA_R = new LLUUID("b5b4a67d-0aee-30d2-72cd-77b333e932ef");
        /// <summary>Agent aiming a bow (left handed)</summary>
        public readonly static LLUUID AIM_BOW_L = new LLUUID("46bb4359-de38-4ed8-6a22-f1f52fe8f506");
        /// <summary>Agent aiming a hand gun (right handed)</summary>
        public readonly static LLUUID AIM_HANDGUN_R = new LLUUID("3147d815-6338-b932-f011-16b56d9ac18b");
        /// <summary>Agent aiming a rifle (right handed)</summary>
        public readonly static LLUUID AIM_RIFLE_R = new LLUUID("ea633413-8006-180a-c3ba-96dd1d756720");
        /// <summary>Agent with angry expression on face</summary>
        public readonly static LLUUID ANGRY = new LLUUID("5747a48e-073e-c331-f6f3-7c2149613d3e");
        /// <summary>Agent hunched over (away)</summary>
        public readonly static LLUUID AWAY = new LLUUID("fd037134-85d4-f241-72c6-4f42164fedee");
        /// <summary>Agent doing a backflip</summary>
        public readonly static LLUUID BACKFLIP = new LLUUID("c4ca6188-9127-4f31-0158-23c4e2f93304");
        /// <summary>Agent laughing while holding belly</summary>
        public readonly static LLUUID BELLY_LAUGH = new LLUUID("18b3a4b5-b463-bd48-e4b6-71eaac76c515");
        /// <summary>Agent blowing a kiss</summary>
        public readonly static LLUUID BLOW_KISS = new LLUUID("db84829b-462c-ee83-1e27-9bbee66bd624");
        /// <summary>Agent with bored expression on face</summary>
        public readonly static LLUUID BORED = new LLUUID("b906c4ba-703b-1940-32a3-0c7f7d791510");
        /// <summary>Agent bowing to audience</summary>
        public readonly static LLUUID BOW = new LLUUID("82e99230-c906-1403-4d9c-3889dd98daba");
        /// <summary>Agent brushing himself/herself off</summary>
        public readonly static LLUUID BRUSH = new LLUUID("349a3801-54f9-bf2c-3bd0-1ac89772af01");
        /// <summary>Agent in busy mode</summary>
        public readonly static LLUUID BUSY = new LLUUID("efcf670c-2d18-8128-973a-034ebc806b67");
        /// <summary>Agent clapping hands</summary>
        public readonly static LLUUID CLAP = new LLUUID("9b0c1c4e-8ac7-7969-1494-28c874c4f668");
        /// <summary>Agent doing a curtsey bow</summary>
        public readonly static LLUUID COURTBOW = new LLUUID("9ba1c942-08be-e43a-fb29-16ad440efc50");
        /// <summary>Agent crouching</summary>
        public readonly static LLUUID CROUCH = new LLUUID("201f3fdf-cb1f-dbec-201f-7333e328ae7c");
        /// <summary>Agent crouching while walking</summary>
        public readonly static LLUUID CROUCHWALK = new LLUUID("47f5f6fb-22e5-ae44-f871-73aaaf4a6022");
        /// <summary>Agent crying</summary>
        public readonly static LLUUID CRY = new LLUUID("92624d3e-1068-f1aa-a5ec-8244585193ed");
        /// <summary>Agent unanimated with arms out (e.g. setting appearance)</summary>
        public readonly static LLUUID CUSTOMIZE = new LLUUID("038fcec9-5ebd-8a8e-0e2e-6e71a0a1ac53");
        /// <summary>Agent re-animated after set appearance finished</summary>
        public readonly static LLUUID CUSTOMIZE_DONE = new LLUUID("6883a61a-b27b-5914-a61e-dda118a9ee2c");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE1 = new LLUUID("b68a3d7c-de9e-fc87-eec8-543d787e5b0d");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE2 = new LLUUID("928cae18-e31d-76fd-9cc9-2f55160ff818");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE3 = new LLUUID("30047778-10ea-1af7-6881-4db7a3a5a114");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE4 = new LLUUID("951469f4-c7b2-c818-9dee-ad7eea8c30b7");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE5 = new LLUUID("4bd69a1d-1114-a0b4-625f-84e0a5237155");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE6 = new LLUUID("cd28b69b-9c95-bb78-3f94-8d605ff1bb12");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE7 = new LLUUID("a54d8ee2-28bb-80a9-7f0c-7afbbe24a5d6");
        /// <summary>Agent dancing</summary>
        public readonly static LLUUID DANCE8 = new LLUUID("b0dc417c-1f11-af36-2e80-7e7489fa7cdc");
        /// <summary>Agent on ground unanimated</summary>
        public readonly static LLUUID DEAD = new LLUUID("57abaae6-1d17-7b1b-5f98-6d11a6411276");
        /// <summary>Agent boozing it up</summary>
        public readonly static LLUUID DRINK = new LLUUID("0f86e355-dd31-a61c-fdb0-3a96b9aad05f");
        /// <summary>Agent with embarassed expression on face</summary>
        public readonly static LLUUID EMBARRASSED = new LLUUID("514af488-9051-044a-b3fc-d4dbf76377c6");
        /// <summary>Agent with afraid expression on face</summary>
        public readonly static LLUUID EXPRESS_AFRAID = new LLUUID("aa2df84d-cf8f-7218-527b-424a52de766e");
        /// <summary>Agent with angry expression on face</summary>
        public readonly static LLUUID EXPRESS_ANGER = new LLUUID("1a03b575-9634-b62a-5767-3a679e81f4de");
        /// <summary>Agent with bored expression on face</summary>
        public readonly static LLUUID EXPRESS_BORED = new LLUUID("214aa6c1-ba6a-4578-f27c-ce7688f61d0d");
        /// <summary>Agent crying</summary>
        public readonly static LLUUID EXPRESS_CRY = new LLUUID("d535471b-85bf-3b4d-a542-93bea4f59d33");
        /// <summary>Agent showing disdain (dislike) for something</summary>
        public readonly static LLUUID EXPRESS_DISDAIN = new LLUUID("d4416ff1-09d3-300f-4183-1b68a19b9fc1");
        /// <summary>Agent with embarassed expression on face</summary>
        public readonly static LLUUID EXPRESS_EMBARRASSED = new LLUUID("0b8c8211-d78c-33e8-fa28-c51a9594e424");
        /// <summary>Agent with frowning expression on face</summary>
        public readonly static LLUUID EXPRESS_FROWN = new LLUUID("fee3df48-fa3d-1015-1e26-a205810e3001");
        /// <summary>Agent with kissy face</summary>
        public readonly static LLUUID EXPRESS_KISS = new LLUUID("1e8d90cc-a84e-e135-884c-7c82c8b03a14");
        /// <summary>Agent expressing laughgter</summary>
        public readonly static LLUUID EXPRESS_LAUGH = new LLUUID("62570842-0950-96f8-341c-809e65110823");
        /// <summary>Agent with open mouth</summary>
        public readonly static LLUUID EXPRESS_OPEN_MOUTH = new LLUUID("d63bc1f9-fc81-9625-a0c6-007176d82eb7");
        /// <summary>Agent with repulsed expression on face</summary>
        public readonly static LLUUID EXPRESS_REPULSED = new LLUUID("f76cda94-41d4-a229-2872-e0296e58afe1");
        /// <summary>Agent expressing sadness</summary>
        public readonly static LLUUID EXPRESS_SAD = new LLUUID("eb6ebfb2-a4b3-a19c-d388-4dd5c03823f7");
        /// <summary>Agent shrugging shoulders</summary>
        public readonly static LLUUID EXPRESS_SHRUG = new LLUUID("a351b1bc-cc94-aac2-7bea-a7e6ebad15ef");
        /// <summary>Agent with a smile</summary>
        public readonly static LLUUID EXPRESS_SMILE = new LLUUID("b7c7c833-e3d3-c4e3-9fc0-131237446312");
        /// <summary>Agent expressing surprise</summary>
        public readonly static LLUUID EXPRESS_SURPRISE = new LLUUID("728646d9-cc79-08b2-32d6-937f0a835c24");
        /// <summary>Agent sticking tongue out</summary>
        public readonly static LLUUID EXPRESS_TONGUE_OUT = new LLUUID("835965c6-7f2f-bda2-5deb-2478737f91bf");
        /// <summary>Agent with big toothy smile</summary>
        public readonly static LLUUID EXPRESS_TOOTHSMILE = new LLUUID("b92ec1a5-e7ce-a76b-2b05-bcdb9311417e");
        /// <summary>Agent winking</summary>
        public readonly static LLUUID EXPRESS_WINK = new LLUUID("da020525-4d94-59d6-23d7-81fdebf33148");
        /// <summary>Agent expressing worry</summary>
        public readonly static LLUUID EXPRESS_WORRY = new LLUUID("9c05e5c7-6f07-6ca4-ed5a-b230390c3950");
        /// <summary>Agent falling down</summary>
        public readonly static LLUUID FALLDOWN = new LLUUID("666307d9-a860-572d-6fd4-c3ab8865c094");
        /// <summary>Agent walking (feminine version)</summary>
        public readonly static LLUUID FEMALE_WALK = new LLUUID("f5fc7433-043d-e819-8298-f519a119b688");
        /// <summary>Agent wagging finger (disapproval)</summary>
        public readonly static LLUUID FINGER_WAG = new LLUUID("c1bc7f36-3ba0-d844-f93c-93be945d644f");
        /// <summary>I'm not sure I want to know</summary>
        public readonly static LLUUID FIST_PUMP = new LLUUID("7db00ccd-f380-f3ee-439d-61968ec69c8a");
        /// <summary>Agent in superman position</summary>
        public readonly static LLUUID FLY = new LLUUID("aec4610c-757f-bc4e-c092-c6e9caf18daf");
        /// <summary>Agent in superman position</summary>
        public readonly static LLUUID FLYSLOW = new LLUUID("2b5a38b2-5e00-3a97-a495-4c826bc443e6");
        /// <summary>Agent greeting another</summary>
        public readonly static LLUUID HELLO = new LLUUID("9b29cd61-c45b-5689-ded2-91756b8d76a9");
        /// <summary>Agent holding bazooka (right handed)</summary>
        public readonly static LLUUID HOLD_BAZOOKA_R = new LLUUID("ef62d355-c815-4816-2474-b1acc21094a6");
        /// <summary>Agent holding a bow (left handed)</summary>
        public readonly static LLUUID HOLD_BOW_L = new LLUUID("8b102617-bcba-037b-86c1-b76219f90c88");
        /// <summary>Agent holding a handgun (right handed)</summary>
        public readonly static LLUUID HOLD_HANDGUN_R = new LLUUID("efdc1727-8b8a-c800-4077-975fc27ee2f2");
        /// <summary>Agent holding a rifle (right handed)</summary>
        public readonly static LLUUID HOLD_RIFLE_R = new LLUUID("3d94bad0-c55b-7dcc-8763-033c59405d33");
        /// <summary>Agent throwing an object (right handed)</summary>
        public readonly static LLUUID HOLD_THROW_R = new LLUUID("7570c7b5-1f22-56dd-56ef-a9168241bbb6");
        /// <summary>Agent in static hover</summary>
        public readonly static LLUUID HOVER = new LLUUID("4ae8016b-31b9-03bb-c401-b1ea941db41d");
        /// <summary>Agent hovering downward</summary>
        public readonly static LLUUID HOVER_DOWN = new LLUUID("20f063ea-8306-2562-0b07-5c853b37b31e");
        /// <summary>Agent hovering upward</summary>
        public readonly static LLUUID HOVER_UP = new LLUUID("62c5de58-cb33-5743-3d07-9e4cd4352864");
        /// <summary>Agent being impatient</summary>
        public readonly static LLUUID IMPATIENT = new LLUUID("5ea3991f-c293-392e-6860-91dfa01278a3");
        /// <summary>Agent jumping</summary>
        public readonly static LLUUID JUMP = new LLUUID("2305bd75-1ca9-b03b-1faa-b176b8a8c49e");
        /// <summary>Agent jumping with fervor</summary>
        public readonly static LLUUID JUMP_FOR_JOY = new LLUUID("709ea28e-1573-c023-8bf8-520c8bc637fa");
        /// <summary>Agent point to lips then rear end</summary>
        public readonly static LLUUID KISS_MY_BUTT = new LLUUID("19999406-3a3a-d58c-a2ac-d72e555dcf51");
        /// <summary>Agent landing from jump, finished flight, etc</summary>
        public readonly static LLUUID LAND = new LLUUID("7a17b059-12b2-41b1-570a-186368b6aa6f");
        /// <summary>Agent laughing</summary>
        public readonly static LLUUID LAUGH_SHORT = new LLUUID("ca5b3f14-3194-7a2b-c894-aa699b718d1f");
        /// <summary>Agent landing from jump, finished flight, etc</summary>
        public readonly static LLUUID MEDIUM_LAND = new LLUUID("f4f00d6e-b9fe-9292-f4cb-0ae06ea58d57");
        /// <summary>Agent sitting on a motorcycle</summary>
        public readonly static LLUUID MOTORCYCLE_SIT = new LLUUID("08464f78-3a8e-2944-cba5-0c94aff3af29");
        /// <summary></summary>
        public readonly static LLUUID MUSCLE_BEACH = new LLUUID("315c3a41-a5f3-0ba4-27da-f893f769e69b");
        /// <summary>Agent moving head side to side</summary>
        public readonly static LLUUID NO = new LLUUID("5a977ed9-7f72-44e9-4c4c-6e913df8ae74");
        /// <summary>Agent moving head side to side with unhappy expression</summary>
        public readonly static LLUUID NO_UNHAPPY = new LLUUID("d83fa0e5-97ed-7eb2-e798-7bd006215cb4");
        /// <summary>Agent taunting another</summary>
        public readonly static LLUUID NYAH_NYAH = new LLUUID("f061723d-0a18-754f-66ee-29a44795a32f");
        /// <summary></summary>
        public readonly static LLUUID ONETWO_PUNCH = new LLUUID("eefc79be-daae-a239-8c04-890f5d23654a");
        /// <summary>Agent giving peace sign</summary>
        public readonly static LLUUID PEACE = new LLUUID("b312b10e-65ab-a0a4-8b3c-1326ea8e3ed9");
        /// <summary>Agent pointing at self</summary>
        public readonly static LLUUID POINT_ME = new LLUUID("17c024cc-eef2-f6a0-3527-9869876d7752");
        /// <summary>Agent pointing at another</summary>
        public readonly static LLUUID POINT_YOU = new LLUUID("ec952cca-61ef-aa3b-2789-4d1344f016de");
        /// <summary>Agent preparing for jump (bending knees)</summary>
        public readonly static LLUUID PRE_JUMP = new LLUUID("7a4e87fe-de39-6fcb-6223-024b00893244");
        /// <summary>Agent punching with left hand</summary>
        public readonly static LLUUID PUNCH_LEFT = new LLUUID("f3300ad9-3462-1d07-2044-0fef80062da0");
        /// <summary>Agent punching with right hand</summary>
        public readonly static LLUUID PUNCH_RIGHT = new LLUUID("c8e42d32-7310-6906-c903-cab5d4a34656");
        /// <summary>Agent acting repulsed</summary>
        public readonly static LLUUID REPULSED = new LLUUID("36f81a92-f076-5893-dc4b-7c3795e487cf");
        /// <summary>Agent trying to be Chuck Norris</summary>
        public readonly static LLUUID ROUNDHOUSE_KICK = new LLUUID("49aea43b-5ac3-8a44-b595-96100af0beda");
        /// <summary>Rocks, Paper, Scissors 1, 2, 3</summary>
        public readonly static LLUUID RPS_COUNTDOWN = new LLUUID("35db4f7e-28c2-6679-cea9-3ee108f7fc7f");
        /// <summary>Agent with hand flat over other hand</summary>
        public readonly static LLUUID RPS_PAPER = new LLUUID("0836b67f-7f7b-f37b-c00a-460dc1521f5a");
        /// <summary>Agent with fist over other hand</summary>
        public readonly static LLUUID RPS_ROCK = new LLUUID("42dd95d5-0bc6-6392-f650-777304946c0f");
        /// <summary>Agent with two fingers spread over other hand</summary>
        public readonly static LLUUID RPS_SCISSORS = new LLUUID("16803a9f-5140-e042-4d7b-d28ba247c325");
        /// <summary>Agent running</summary>
        public readonly static LLUUID RUN = new LLUUID("05ddbff8-aaa9-92a1-2b74-8fe77a29b445");
        /// <summary>Agent appearing sad</summary>
        public readonly static LLUUID SAD = new LLUUID("0eb702e2-cc5a-9a88-56a5-661a55c0676a");
        /// <summary>Agent saluting</summary>
        public readonly static LLUUID SALUTE = new LLUUID("cd7668a6-7011-d7e2-ead8-fc69eff1a104");
        /// <summary>Agent shooting bow (left handed)</summary>
        public readonly static LLUUID SHOOT_BOW_L = new LLUUID("e04d450d-fdb5-0432-fd68-818aaf5935f8");
        /// <summary>Agent cupping mouth as if shouting</summary>
        public readonly static LLUUID SHOUT = new LLUUID("6bd01860-4ebd-127a-bb3d-d1427e8e0c42");
        /// <summary>Agent shrugging shoulders</summary>
        public readonly static LLUUID SHRUG = new LLUUID("70ea714f-3a97-d742-1b01-590a8fcd1db5");
        /// <summary>Agent in sit position</summary>
        public readonly static LLUUID SIT = new LLUUID("1a5fe8ac-a804-8a5d-7cbd-56bd83184568");
        /// <summary>Agent in sit position (feminine)</summary>
        public readonly static LLUUID SIT_FEMALE = new LLUUID("b1709c8d-ecd3-54a1-4f28-d55ac0840782");
        /// <summary>Agent in sit position (generic)</summary>
        public readonly static LLUUID SIT_GENERIC = new LLUUID("245f3c54-f1c0-bf2e-811f-46d8eeb386e7");
        /// <summary>Agent sitting on ground</summary>
        public readonly static LLUUID SIT_GROUND = new LLUUID("1c7600d6-661f-b87b-efe2-d7421eb93c86");
        /// <summary>Agent sitting on ground</summary>
        public readonly static LLUUID SIT_GROUND_staticRAINED = new LLUUID("1a2bd58e-87ff-0df8-0b4c-53e047b0bb6e");
        /// <summary></summary>
        public readonly static LLUUID SIT_TO_STAND = new LLUUID("a8dee56f-2eae-9e7a-05a2-6fb92b97e21e");
        /// <summary>Agent sleeping on side</summary>
        public readonly static LLUUID SLEEP = new LLUUID("f2bed5f9-9d44-39af-b0cd-257b2a17fe40");
        /// <summary>Agent smoking</summary>
        public readonly static LLUUID SMOKE_IDLE = new LLUUID("d2f2ee58-8ad1-06c9-d8d3-3827ba31567a");
        /// <summary>Agent inhaling smoke</summary>
        public readonly static LLUUID SMOKE_INHALE = new LLUUID("6802d553-49da-0778-9f85-1599a2266526");
        /// <summary></summary>
        public readonly static LLUUID SMOKE_THROW_DOWN = new LLUUID("0a9fb970-8b44-9114-d3a9-bf69cfe804d6");
        /// <summary>Agent taking a picture</summary>
        public readonly static LLUUID SNAPSHOT = new LLUUID("eae8905b-271a-99e2-4c0e-31106afd100c");
        /// <summary>Agent standing</summary>
        public readonly static LLUUID STAND = new LLUUID("2408fe9e-df1d-1d7d-f4ff-1384fa7b350f");
        /// <summary>Agent standing up</summary>
        public readonly static LLUUID STANDUP = new LLUUID("3da1d753-028a-5446-24f3-9c9b856d9422");
        /// <summary>Agent standing</summary>
        public readonly static LLUUID STAND_1 = new LLUUID("15468e00-3400-bb66-cecc-646d7c14458e");
        /// <summary>Agent standing</summary>
        public readonly static LLUUID STAND_2 = new LLUUID("370f3a20-6ca6-9971-848c-9a01bc42ae3c");
        /// <summary>Agent standing</summary>
        public readonly static LLUUID STAND_3 = new LLUUID("42b46214-4b44-79ae-deb8-0df61424ff4b");
        /// <summary>Agent standing</summary>
        public readonly static LLUUID STAND_4 = new LLUUID("f22fed8b-a5ed-2c93-64d5-bdd8b93c889f");
        /// <summary>Agent stretching</summary>
        public readonly static LLUUID STRETCH = new LLUUID("80700431-74ec-a008-14f8-77575e73693f");
        /// <summary>Agent in stride (fast walk)</summary>
        public readonly static LLUUID STRIDE = new LLUUID("1cb562b0-ba21-2202-efb3-30f82cdf9595");
        /// <summary>Agent surfing</summary>
        public readonly static LLUUID SURF = new LLUUID("41426836-7437-7e89-025d-0aa4d10f1d69");
        /// <summary>Agent acting surprised</summary>
        public readonly static LLUUID SURPRISE = new LLUUID("313b9881-4302-73c0-c7d0-0e7a36b6c224");
        /// <summary>Agent striking with a sword</summary>
        public readonly static LLUUID SWORD_STRIKE = new LLUUID("85428680-6bf9-3e64-b489-6f81087c24bd");
        /// <summary>Agent talking (lips moving)</summary>
        public readonly static LLUUID TALK = new LLUUID("5c682a95-6da4-a463-0bf6-0f5b7be129d1");
        /// <summary>Agent throwing a tantrum</summary>
        public readonly static LLUUID TANTRUM = new LLUUID("11000694-3f41-adc2-606b-eee1d66f3724");
        /// <summary>Agent throwing an object (right handed)</summary>
        public readonly static LLUUID THROW_R = new LLUUID("aa134404-7dac-7aca-2cba-435f9db875ca");
        /// <summary>Agent trying on a shirt</summary>
        public readonly static LLUUID TRYON_SHIRT = new LLUUID("83ff59fe-2346-f236-9009-4e3608af64c1");
        /// <summary>Agent turning to the left</summary>
        public readonly static LLUUID TURNLEFT = new LLUUID("56e0ba0d-4a9f-7f27-6117-32f2ebbf6135");
        /// <summary>Agent turning to the right</summary>
        public readonly static LLUUID TURNRIGHT = new LLUUID("2d6daa51-3192-6794-8e2e-a15f8338ec30");
        /// <summary>Agent typing</summary>
        public readonly static LLUUID TYPE = new LLUUID("c541c47f-e0c0-058b-ad1a-d6ae3a4584d9");
        /// <summary>Agent walking</summary>
        public readonly static LLUUID WALK = new LLUUID("6ed24bd8-91aa-4b12-ccc7-c97c857ab4e0");
        /// <summary>Agent whispering</summary>
        public readonly static LLUUID WHISPER = new LLUUID("7693f268-06c7-ea71-fa21-2b30d6533f8f");
        /// <summary>Agent whispering with fingers in mouth</summary>
        public readonly static LLUUID WHISTLE = new LLUUID("b1ed7982-c68e-a982-7561-52a88a5298c0");
        /// <summary>Agent winking</summary>
        public readonly static LLUUID WINK = new LLUUID("869ecdad-a44b-671e-3266-56aef2e3ac2e");
        /// <summary>Agent winking</summary>
        public readonly static LLUUID WINK_HOLLYWOOD = new LLUUID("c0c4030f-c02b-49de-24ba-2331f43fe41c");
        /// <summary>Agent worried</summary>
        public readonly static LLUUID WORRY = new LLUUID("9f496bd2-589a-709f-16cc-69bf7df1d36c");
        /// <summary>Agent nodding yes</summary>
        public readonly static LLUUID YES = new LLUUID("15dd911d-be82-2856-26db-27659b142875");
        /// <summary>Agent nodding yes with happy face</summary>
        public readonly static LLUUID YES_HAPPY = new LLUUID("b8c8b2a3-9008-1771-3bfc-90924955ab2d");
        /// <summary>Agent floating with legs and arms crossed</summary>
        public readonly static LLUUID YOGA_FLOAT = new LLUUID("42ecd00b-9947-a97c-400a-bbc9174c7aeb");
    }
}
