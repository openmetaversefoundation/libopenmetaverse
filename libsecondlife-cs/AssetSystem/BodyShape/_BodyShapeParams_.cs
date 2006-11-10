using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using libsecondlife.AssetSystem.BodyShape;

namespace libsecondlife.AssetSystem.BodyShape
{
    class BodyShapeParams
    {
        public string GetName( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return "Male_Skeleton";
                case 33:
                    return "Height";
                case 34:
                    return "Thickness";
                case 36:
                    return "Shoulders";
                case 37:
                    return "Hip Width";
                case 842:
                    return "Hip Length";
                case 38:
                    return "Torso Length";
                case 195:
                    return "EyeBone_Spread";
                case 661:
                    return "EyeBone_Head_Shear";
                case 772:
                    return "EyeBone_Head_Elongate";
                case 768:
                    return "EyeBone_Bug";
                case 655:
                    return "Head Size";
                case 197:
                    return "Shoe_Heels";
                case 502:
                    return "Shoe_Platform";
                case 675:
                    return "Hand Size";
                case 683:
                    return "Neck Thickness";
                case 689:
                    return "EyeBone_Big_Eyes";
                case 692:
                    return "Leg Length";
                case 693:
                    return "Arm Length";
                case 756:
                    return "Neck Length";
                case 180:
                    return "Hair_Volume";
                case 761:
                    return "Hair_Volume_Small";
                case 181:
                    return "Hair_Big_Front";
                case 182:
                    return "Hair_Big_Top";
                case 183:
                    return "Hair_Big_Back";
                case 184:
                    return "Hair_Spiked";
                case 140:
                    return "Hair_Part_Middle";
                case 141:
                    return "Hair_Part_Right";
                case 142:
                    return "Hair_Part_Left";
                case 143:
                    return "Hair_Sides_Full";
                case 144:
                    return "Bangs_Front_Up";
                case 145:
                    return "Bangs_Front_Down";
                case 146:
                    return "Bangs_Sides_Up";
                case 147:
                    return "Bangs_Sides_Down";
                case 148:
                    return "Bangs_Back_Up";
                case 149:
                    return "Bangs_Back_Down";
                case 171:
                    return "Hair_Front_Down";
                case 172:
                    return "Hair_Front_Up";
                case 173:
                    return "Hair_Sides_Down";
                case 174:
                    return "Hair_Sides_Up";
                case 175:
                    return "Hair_Back_Down";
                case 176:
                    return "Hair_Back_Up";
                case 177:
                    return "Hair_Rumpled";
                case 178:
                    return "Hair_Swept_Back";
                case 179:
                    return "Hair_Swept_Forward";
                case 190:
                    return "Hair_Tilt_Right";
                case 191:
                    return "Hair_Tilt_Left";
                case 192:
                    return "Bangs_Part_Middle";
                case 674:
                    return "Hair_Shear_Back";
                case 762:
                    return "Hair_Shear_Front";
                case 754:
                    return "Hair_Taper_Back";
                case 755:
                    return "Hair_Taper_Front";
                case 1:
                    return "Big_Brow";
                case 2:
                    return "Nose_Big_Out";
                case 4:
                    return "Broad_Nostrils";
                case 759:
                    return "Low_Septum_Nose";
                case 517:
                    return "Wide_Nose";
                case 5:
                    return "Cleft_Chin";
                case 6:
                    return "Bulbous_Nose_Tip";
                case 7:
                    return "Weak_Chin";
                case 8:
                    return "Double_Chin";
                case 10:
                    return "Sunken_Cheeks";
                case 11:
                    return "Noble_Nose_Bridge";
                case 758:
                    return "Lower_Bridge_Nose";
                case 12:
                    return "Jowls";
                case 13:
                    return "Cleft_Chin_Upper";
                case 14:
                    return "High_Cheek_Bones";
                case 15:
                    return "Ears_Out";
                case 870:
                    return "Pointy_Eyebrows";
                case 17:
                    return "Square_Jaw";
                case 18:
                    return "Puffy_Upper_Cheeks";
                case 19:
                    return "Upturned_Nose_Tip";
                case 20:
                    return "Bulbous_Nose";
                case 21:
                    return "Upper_Eyelid_Fold";
                case 22:
                    return "Attached_Earlobes";
                case 23:
                    return "Baggy_Eyes";
                case 765:
                    return "Puffy_Lower_Lids";
                case 24:
                    return "Wide_Eyes";
                case 25:
                    return "Wide_Lip_Cleft";
                case 764:
                    return "Lip_Cleft_Deep";
                case 27:
                    return "Wide_Nose_Bridge";
                case 872:
                    return "Arced_Eyebrows";
                case 871:
                    return "Lower_Eyebrows";
                case 35:
                    return "Big_Ears";
                case 796:
                    return "Pointy_Ears";
                case 185:
                    return "Deep_Chin";
                case 186:
                    return "Egg_Head";
                case 187:
                    return "Squash_Stretch_Head";
                case 188:
                    return "Square_Head";
                case 189:
                    return "Round_Head";
                case 194:
                    return "Eye_Spread";
                case 400:
                    return "Displace_Hair_Facial";
                case 506:
                    return "Mouth_Height";
                case 633:
                    return "Fat_Head";
                case 630:
                    return "Forehead_Round";
                case 631:
                    return "Forehead_Slant";
                case 650:
                    return "Eyelid_Corner_Up";
                case 880:
                    return "Eyelid_Inner_Corner_Up";
                case 653:
                    return "Tall_Lips";
                case 656:
                    return "Crooked_Nose";
                case 657:
                    return "Smile_Mouth";
                case 658:
                    return "Frown_Mouth";
                case 797:
                    return "Fat_Upper_Lip";
                case 798:
                    return "Fat_Lower_Lip";
                case 660:
                    return "Shear_Head";
                case 770:
                    return "Elongate_Head";
                case 663:
                    return "Shift_Mouth";
                case 664:
                    return "Pop_Eye";
                case 760:
                    return "Jaw_Angle";
                case 665:
                    return "Jaw_Jut";
                case 686:
                    return "Head_Eyes_Big";
                case 767:
                    return "Bug_Eyed_Head";
                case 518:
                    return "Eyelashes_Long";
                case 626:
                    return "Big_Chest";
                case 627:
                    return "Small_Chest";
                case 843:
                    return "No_Chest";
                case 106:
                    return "Muscular_Torso";
                case 648:
                    return "Scrawny_Torso";
                case 677:
                    return "Scrawny_Torso_Male";
                case 634:
                    return "Fat_Torso";
                case 507:
                    return "Breast_Gravity";
                case 628:
                    return "Displace_Loose_Upperbody";
                case 840:
                    return "Shirtsleeve_flair";
                case 684:
                    return "Breast_Female_Cleavage";
                case 685:
                    return "Chest_Male_No_Pecs";
                case 100:
                    return "Male_Torso";
                case 151:
                    return "Big_Butt_Legs";
                case 794:
                    return "Small_Butt";
                case 152:
                    return "Muscular_Legs";
                case 651:
                    return "Scrawny_Legs";
                case 853:
                    return "Bowed_Legs";
                case 500:
                    return "Shoe_Heel_Height";
                case 501:
                    return "Shoe_Platform_Height";
                case 508:
                    return "Shoe_Platform_Width";
                case 509:
                    return "Shoe_Heel_Point";
                case 510:
                    return "Shoe_Heel_Thick";
                case 511:
                    return "Shoe_Toe_Point";
                case 512:
                    return "Shoe_Toe_Square";
                case 654:
                    return "Shoe_Toe_Thick";
                case 515:
                    return "Foot_Size";
                case 516:
                    return "Displace_Loose_Lowerbody";
                case 625:
                    return "Leg_Pantflair";
                case 793:
                    return "Leg_Longcuffs";
                case 638:
                    return "Low_Crotch";
                case 635:
                    return "Fat_Legs";
                case 879:
                    return "Male_Package";
                case 679:
                    return "Eyeball_Size";
                case 687:
                    return "Eyeball_Size";
                case 694:
                    return "Eyeball_Size";
                case 695:
                    return "Eyeball_Size";
                case 680:
                    return "Eyeball_Size";
                case 688:
                    return "Eyeball_Size";
                case 681:
                    return "Eyeball_Size";
                case 691:
                    return "Eyeball_Size";
                case 845:
                    return "skirt_poofy";
                case 846:
                    return "skirt_loose";
                case 866:
                    return "skirt_tight";
                case 867:
                    return "skirt_smallbutt";
                case 848:
                    return "skirt_bustle";
                case 847:
                    return "skirt_bowlegs";
                case 852:
                    return "skirt_bigbutt";
                case 849:
                    return "skirt_belly";
                case 851:
                    return "skirt_chubby";
                case 856:
                    return "skirt_lovehandles";
                case 111:
                    return "Pigment";
                case 110:
                    return "Red Skin";
                case 108:
                    return "Rainbow Color";
                case 114:
                    return "Blonde Hair";
                case 113:
                    return "Red Hair";
                case 115:
                    return "White Hair";
                case 112:
                    return "Rainbow Color";
                case 99:
                    return "Eye Color";
                case 98:
                    return "Eye Lightness";
                case 116:
                    return "Rosy Complexion";
                case 117:
                    return "Lip Pinkness";
                case 165:
                    return "Freckles";
                case 700:
                    return "Lipstick Color";
                case 701:
                    return "Lipstick";
                case 702:
                    return "Lipgloss";
                case 704:
                    return "Blush";
                case 705:
                    return "Blush Color";
                case 711:
                    return "Blush Opacity";
                case 708:
                    return "Out Shdw Color";
                case 706:
                    return "Out Shdw Opacity";
                case 707:
                    return "Outer Shadow";
                case 712:
                    return "In Shdw Color";
                case 713:
                    return "In Shdw Opacity";
                case 709:
                    return "Inner Shadow";
                case 703:
                    return "Eyeliner";
                case 714:
                    return "Eyeliner Color";
                case 751:
                    return "5 O'Clock Shadow";
                case 1048:
                    return "Collar Back";
                case 710:
                    return "Nail Polish";
                case 715:
                    return "Nail Polish Color";
                case 620:
                    return "bottom length upper";
                case 622:
                    return "open upper";
                case 621:
                    return "bottom length lower";
                case 623:
                    return "open lower";
                case 858:
                    return "Skirt Length";
                case 859:
                    return "Slit Front";
                case 860:
                    return "Slit Back";
                case 861:
                    return "Slit Left";
                case 862:
                    return "Slit Right";
                case 828:
                    return "Loose Upper Clothing";
                case 816:
                    return "Loose Lower Clothing";
                case 814:
                    return "Waist Height";
                case 815:
                    return "Pants Length";
                case 800:
                    return "Sleeve Length";
                case 801:
                    return "Shirt Bottom";
                case 802:
                    return "Collar Front";
                case 781:
                    return "Collar Back";
                case 150:
                    return "Body Definition";
                case 775:
                    return "Body Freckles";
                case 162:
                    return "Facial Definition";
                case 163:
                    return "wrinkles";
                case 505:
                    return "Lip Thickness";
                case 799:
                    return "Lip Ratio";
                case 155:
                    return "Lip Width";
                case 196:
                    return "Eye Spacing";
                case 769:
                    return "Eye Depth";
                case 198:
                    return "Heel Height";
                case 513:
                    return "Heel Shape";
                case 514:
                    return "Toe Shape";
                case 503:
                    return "Platform Height";
                case 193:
                    return "Head Shape";
                case 157:
                    return "Belly Size";
                case 637:
                    return "Body Fat";
                case 130:
                    return "Front Fringe";
                case 131:
                    return "Side Fringe";
                case 132:
                    return "Back Fringe";
                case 133:
                    return "Hair Front";
                case 134:
                    return "Hair Sides";
                case 135:
                    return "Hair Back";
                case 136:
                    return "Hair Sweep";
                case 137:
                    return "Hair Tilt";
                case 608:
                    return "bottom length lower";
                case 609:
                    return "open jacket";
                case 105:
                    return "Breast Size";
                case 629:
                    return "Forehead Angle";
                case 646:
                    return "Egg_Head";
                case 647:
                    return "Squash_Stretch_Head";
                case 649:
                    return "Torso Muscles";
                case 678:
                    return "Torso Muscles";
                case 652:
                    return "Leg Muscles";
                case 659:
                    return "Mouth Corner";
                case 662:
                    return "Face Shear";
                case 773:
                    return "Head Length";
                case 682:
                    return "Head Size";
                case 690:
                    return "Eye Size";
                case 752:
                    return "Hair Thickness";
                case 763:
                    return "Hair Volume";
                case 785:
                    return "Pigtails";
                case 789:
                    return "Ponytail";
                case 795:
                    return "Butt Size";
                case 841:
                    return "Bowed_Legs";
                case 753:
                    return "Saddlebags";
                case 676:
                    return "Love_Handles";
                case 863:
                    return "skirt_looseness";
                case 119:
                    return "Eyebrow Size";
                case 750:
                    return "Eyebrow Density";
                case 166:
                    return "Sideburns";
                case 167:
                    return "Moustache";
                case 168:
                    return "Soulpatch";
                case 169:
                    return "Chin Curtains";
                case 606:
                    return "Sleeve Length";
                case 607:
                    return "Collar Front";
                case 780:
                    return "Collar Back";
                case 603:
                    return "Sleeve Length";
                case 604:
                    return "Bottom";
                case 605:
                    return "Collar Front";
                case 779:
                    return "Collar Back";
                case 617:
                    return "Socks Length";
                case 616:
                    return "Shoe Height";
                case 619:
                    return "Pants Length";
                case 624:
                    return "Pants Waist";
                case 93:
                    return "Glove Length";
                case 844:
                    return "Glove Fingers";
                case 16:
                    return "Pointy_Eyebrows";
                case 757:
                    return "Lower_Eyebrows";
                case 31:
                    return "Arced_Eyebrows";
                case 877:
                    return "Jacket Wrinkles";
            }
        }
        public string GetLabel( uint Param )
        {
            switch( Param )
            {
                default:
                    return "";
                case 33:
                    return "Height";
                case 34:
                    return "Body Thickness";
                case 36:
                    return "Shoulders";
                case 37:
                    return "Hip Width";
                case 655:
                    return "Head Size";
                case 180:
                    return "Hair Volume";
                case 761:
                    return "Hair Volume";
                case 181:
                    return "Big Hair Front";
                case 182:
                    return "Big Hair Top";
                case 183:
                    return "Big Hair Back";
                case 184:
                    return "Spiked Hair";
                case 140:
                    return "Middle Part";
                case 141:
                    return "Right Part";
                case 142:
                    return "Left Part";
                case 143:
                    return "Full Hair Sides";
                case 144:
                    return "Front Bangs Up";
                case 145:
                    return "Front Bangs Down";
                case 146:
                    return "Side Bangs Up";
                case 147:
                    return "Side Bangs Down";
                case 148:
                    return "Back Bangs Up";
                case 149:
                    return "Back Bangs Down";
                case 171:
                    return "Front Hair Down";
                case 172:
                    return "Front Hair Up";
                case 173:
                    return "Sides Hair Down";
                case 174:
                    return "Sides Hair Up";
                case 175:
                    return "Back Hair Down";
                case 176:
                    return "Back Hair Up";
                case 177:
                    return "Rumpled Hair";
                case 178:
                    return "Swept Back Hair";
                case 179:
                    return "Swept Forward Hair";
                case 190:
                    return "Hair Tilted Right";
                case 191:
                    return "Hair Tilted Left";
                case 192:
                    return "Part Bangs";
                case 674:
                    return "Shear Back";
                case 762:
                    return "Shear Front";
                case 754:
                    return "Taper Back";
                case 755:
                    return "Taper Front";
                case 1:
                    return "Brow Size";
                case 2:
                    return "Nose Size";
                case 4:
                    return "Nostril Width";
                case 759:
                    return "Nostril Division";
                case 517:
                    return "Nose Width";
                case 5:
                    return "Chin Cleft";
                case 6:
                    return "Nose Tip Shape";
                case 7:
                    return "Chin Angle";
                case 8:
                    return "Chin-Neck";
                case 10:
                    return "Lower Cheeks";
                case 11:
                    return "Upper Bridge";
                case 758:
                    return "Lower Bridge";
                case 13:
                    return "Upper Chin Cleft";
                case 14:
                    return "Cheek Bones";
                case 15:
                    return "Ear Angle";
                case 870:
                    return "Eyebrow Points";
                case 17:
                    return "Jaw Shape";
                case 18:
                    return "Upper Cheeks";
                case 19:
                    return "Nose Tip Angle";
                case 20:
                    return "Nose Thickness";
                case 21:
                    return "Upper Eyelid Fold";
                case 22:
                    return "Attached Earlobes";
                case 23:
                    return "Eye Bags";
                case 765:
                    return "Puffy Eyelids";
                case 24:
                    return "Eye Opening";
                case 25:
                    return "Lip Cleft";
                case 764:
                    return "Lip Cleft Depth";
                case 27:
                    return "Bridge Width";
                case 872:
                    return "Eyebrow Arc";
                case 871:
                    return "Eyebrow Height";
                case 35:
                    return "Ear Size";
                case 796:
                    return "Ear Tips";
                case 185:
                    return "Chin Depth";
                case 186:
                    return "Egg Head";
                case 187:
                    return "Squash/Stretch Head";
                case 400:
                    return "Hair Thickess";
                case 506:
                    return "Mouth Position";
                case 633:
                    return "Fat Head";
                case 630:
                    return "Round Forehead";
                case 631:
                    return "Slanted Forehead";
                case 650:
                    return "Outer Eye Corner";
                case 880:
                    return "Inner Eye Corner";
                case 653:
                    return "Lip Fullness";
                case 656:
                    return "Crooked Nose";
                case 657:
                    return "Mouth Corner";
                case 658:
                    return "Mouth Corner";
                case 797:
                    return "Fat Upper Lip";
                case 798:
                    return "Fat Lower Lip";
                case 660:
                    return "Shear Face";
                case 770:
                    return "Shear Face";
                case 663:
                    return "Shift Mouth";
                case 664:
                    return "Eye Pop";
                case 760:
                    return "Jaw Angle";
                case 665:
                    return "Jaw Jut";
                case 686:
                    return "Eye Size";
                case 767:
                    return "Eye Depth";
                case 518:
                    return "Eyelash Length";
                case 626:
                    return "Chest Size";
                case 627:
                    return "Chest Size";
                case 843:
                    return "Chest Size";
                case 106:
                    return "Torso Muscles";
                case 648:
                    return "Torso Muscles";
                case 677:
                    return "Torso Scrawny";
                case 634:
                    return "Fat Torso";
                case 507:
                    return "Breast Buoyancy";
                case 628:
                    return "Shirt Fit";
                case 840:
                    return "Sleeve Looseness";
                case 684:
                    return "Breast Cleavage";
                case 685:
                    return "Pectorals";
                case 151:
                    return "Butt Size";
                case 794:
                    return "Butt Size";
                case 152:
                    return "Leg Muscles";
                case 651:
                    return "Scrawny Leg";
                case 853:
                    return "Knee Angle";
                case 500:
                    return "Heel Height";
                case 501:
                    return "Platform Height";
                case 508:
                    return "Platform Width";
                case 509:
                    return "Heel Shape";
                case 510:
                    return "Heel Shape";
                case 511:
                    return "Toe Shape";
                case 512:
                    return "Toe Shape";
                case 654:
                    return "Toe Thickness";
                case 515:
                    return "Foot Size";
                case 516:
                    return "Pants Fit";
                case 625:
                    return "Cuff Flare";
                case 793:
                    return "Longcuffs";
                case 638:
                    return "Pants Crotch";
                case 635:
                    return "Fat Torso";
                case 879:
                    return "Package";
                case 679:
                    return "Eyeball Size";
                case 687:
                    return "Big Eyeball";
                case 694:
                    return "Eyeball Size";
                case 695:
                    return "Big Eyeball";
                case 680:
                    return "Eyeball Size";
                case 688:
                    return "Big Eyeball";
                case 681:
                    return "Eyeball Size";
                case 691:
                    return "Big Eyeball";
                case 845:
                    return "poofy skirt";
                case 846:
                    return "loose skirt";
                case 866:
                    return "tight skirt";
                case 867:
                    return "tight skirt";
                case 848:
                    return "bustle skirt";
                case 847:
                    return "legs skirt";
                case 852:
                    return "bigbutt skirt";
                case 849:
                    return "big belly skirt";
                case 110:
                    return "Ruddiness";
                case 828:
                    return "Shirt Fit";
                case 816:
                    return "Pants Fit";
                case 799:
                    return "Lip Ratio";
                case 155:
                    return "Lip Width";
                case 196:
                    return "Eye Spacing";
                case 193:
                    return "Head Shape";
                case 608:
                    return "Jacket Length";
                case 609:
                    return "Open Front";
                case 646:
                    return "Egg Head";
                case 647:
                    return "Head Stretch";
                case 649:
                    return "Torso Muscles";
                case 682:
                    return "Head Size";
                case 690:
                    return "Eye Size";
                case 795:
                    return "Butt Size";
                case 841:
                    return "Knee Angle";
                case 753:
                    return "Saddle Bags";
                case 676:
                    return "Love Handles";
                case 863:
                    return "Skirt Fit";
                case 16:
                    return "Eyebrow Points";
                case 757:
                    return "Eyebrow Height";
                case 31:
                    return "Eyebrow Arc";
                case 877:
                    return "Jacket Wrinkles";
            }
        }
        public string GetLabelMin( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return "Female";
                case 33:
                    return "Short";
                case 34:
                    return "Body Thin";
                case 36:
                    return "Narrow";
                case 37:
                    return "Narrow";
                case 842:
                    return "Short hips";
                case 38:
                    return "Short Torso";
                case 195:
                    return "Eyes Together";
                case 661:
                    return "Eyes Shear Left Up";
                case 772:
                    return "Eyes Short Head";
                case 768:
                    return "Eyes Sunken";
                case 655:
                    return "Small Head";
                case 197:
                    return "No Heels";
                case 502:
                    return "No Heels";
                case 675:
                    return "Small Hands";
                case 683:
                    return "Skinny Neck";
                case 689:
                    return "Eyes Back";
                case 692:
                    return "Short Legs";
                case 693:
                    return "Short Arms";
                case 756:
                    return "Short Neck";
                case 180:
                    return "Less";
                case 761:
                    return "Less";
                case 181:
                    return "Less";
                case 182:
                    return "Less";
                case 183:
                    return "Less";
                case 184:
                    return "No Spikes";
                case 140:
                    return "No Part";
                case 141:
                    return "No Part";
                case 142:
                    return "No Part";
                case 143:
                    return "Mowhawk";
                case 144:
                    return "Bangs";
                case 145:
                    return "Bangs";
                case 146:
                    return "Side Bangs";
                case 147:
                    return "Side Bangs";
                case 148:
                    return "Back Bangs";
                case 149:
                    return "Back Bangs";
                case 171:
                    return "Front Hair";
                case 172:
                    return "Front Hair";
                case 173:
                    return "Sides Hair";
                case 174:
                    return "Sides Hair";
                case 175:
                    return "Back Hair";
                case 176:
                    return "Back Hair";
                case 177:
                    return "Smooth Hair";
                case 178:
                    return "NotHair";
                case 179:
                    return "Hair";
                case 190:
                    return "Hair";
                case 191:
                    return "Hair";
                case 192:
                    return "No Part";
                case 674:
                    return "Full Back";
                case 762:
                    return "Full Front";
                case 754:
                    return "Wide Back";
                case 755:
                    return "Wide Front";
                case 1:
                    return "Small";
                case 2:
                    return "Small";
                case 4:
                    return "Narrow";
                case 759:
                    return "High";
                case 517:
                    return "Narrow";
                case 5:
                    return "Round";
                case 6:
                    return "Pointy";
                case 7:
                    return "Chin Out";
                case 8:
                    return "Tight Chin";
                case 10:
                    return "Well-Fed";
                case 11:
                    return "Low";
                case 758:
                    return "Low";
                case 12:
                    return "Less";
                case 13:
                    return "Round";
                case 14:
                    return "Low";
                case 15:
                    return "In";
                case 870:
                    return "Smooth";
                case 17:
                    return "Pointy";
                case 18:
                    return "Thin";
                case 19:
                    return "Downturned";
                case 20:
                    return "Thin Nose";
                case 21:
                    return "Uncreased";
                case 22:
                    return "Unattached";
                case 23:
                    return "Smooth";
                case 765:
                    return "Flat";
                case 24:
                    return "Narrow";
                case 25:
                    return "Narrow";
                case 764:
                    return "Shallow";
                case 27:
                    return "Narrow";
                case 872:
                    return "Flat";
                case 871:
                    return "Higher";
                case 35:
                    return "Small";
                case 796:
                    return "Flat";
                case 185:
                    return "Shallow";
                case 186:
                    return "Chin Heavy";
                case 187:
                    return "Squash Head";
                case 188:
                    return "Less Square";
                case 189:
                    return "Less Round";
                case 194:
                    return "Eyes Together";
                case 400:
                    return "Cropped Hair";
                case 506:
                    return "High";
                case 633:
                    return "Skinny";
                case 630:
                    return "Less";
                case 631:
                    return "Less";
                case 650:
                    return "Corner Down";
                case 880:
                    return "Corner Down";
                case 653:
                    return "Less Full";
                case 656:
                    return "Nose Left";
                case 657:
                    return "Corner Normal";
                case 658:
                    return "Corner Normal";
                case 797:
                    return "Normal Upper";
                case 798:
                    return "Normal Lower";
                case 660:
                    return "Shear Left";
                case 770:
                    return "Flat Head";
                case 663:
                    return "Shift Left";
                case 664:
                    return "Pop Right Eye";
                case 760:
                    return "Low Jaw";
                case 665:
                    return "Overbite";
                case 686:
                    return "Beady Eyes";
                case 767:
                    return "Sunken Eyes";
                case 518:
                    return "Short";
                case 626:
                    return "Small";
                case 627:
                    return "Large";
                case 843:
                    return "Some";
                case 106:
                    return "Regular";
                case 648:
                    return "Regular";
                case 677:
                    return "Regular";
                case 634:
                    return "skinny";
                case 507:
                    return "Less Gravity";
                case 840:
                    return "Tight Sleeves";
                case 684:
                    return "Separate";
                case 685:
                    return "Big Pectorals";
                case 100:
                    return "Male_Torso";
                case 151:
                    return "Regular";
                case 794:
                    return "Regular";
                case 152:
                    return "Regular Muscles";
                case 651:
                    return "Regular Muscles";
                case 500:
                    return "Low Heels";
                case 501:
                    return "Low Platforms";
                case 508:
                    return "Narrow";
                case 509:
                    return "Default Heels";
                case 510:
                    return "default Heels";
                case 511:
                    return "Default Toe";
                case 512:
                    return "Default Toe";
                case 654:
                    return "Flat Toe";
                case 515:
                    return "Small";
                case 625:
                    return "Tight Cuffs";
                case 638:
                    return "High and Tight";
                case 635:
                    return "skinny";
                case 879:
                    return "Coin Purse";
                case 679:
                    return "small eye";
                case 687:
                    return "small eye";
                case 694:
                    return "small eye";
                case 695:
                    return "small eye";
                case 680:
                    return "small eye";
                case 688:
                    return "small eye";
                case 681:
                    return "small eye";
                case 691:
                    return "small eye";
                case 845:
                    return "less poofy";
                case 846:
                    return "form fitting";
                case 866:
                    return "form fitting";
                case 867:
                    return "form fitting";
                case 848:
                    return "no bustle";
                case 852:
                    return "less";
                case 851:
                    return "less";
                case 856:
                    return "less";
                case 111:
                    return "Light";
                case 110:
                    return "Pale";
                case 108:
                    return "None";
                case 114:
                    return "Black";
                case 113:
                    return "No Red";
                case 115:
                    return "No White";
                case 112:
                    return "None";
                case 99:
                    return "Natural";
                case 98:
                    return "Darker";
                case 116:
                    return "Less Rosy";
                case 117:
                    return "Darker";
                case 165:
                    return "Less";
                case 700:
                    return "Pink";
                case 701:
                    return "No Lipstick";
                case 702:
                    return "No Lipgloss";
                case 704:
                    return "No Blush";
                case 705:
                    return "Pink";
                case 711:
                    return "Clear";
                case 708:
                    return "Light";
                case 706:
                    return "Clear";
                case 707:
                    return "No Eyeshadow";
                case 712:
                    return "Light";
                case 713:
                    return "Clear";
                case 709:
                    return "No Eyeshadow";
                case 703:
                    return "No Eyeliner";
                case 714:
                    return "Dark Green";
                case 751:
                    return "Dense hair";
                case 1048:
                    return "Low";
                case 710:
                    return "No Polish";
                case 715:
                    return "Pink";
                case 620:
                    return "hi cut";
                case 622:
                    return "closed";
                case 621:
                    return "hi cut";
                case 623:
                    return "open";
                case 858:
                    return "Short";
                case 859:
                    return "Open Front";
                case 860:
                    return "Open Back";
                case 861:
                    return "Open Left";
                case 862:
                    return "Open Right";
                case 828:
                    return "Tight Shirt";
                case 816:
                    return "Tight Pants";
                case 814:
                    return "Low";
                case 815:
                    return "Short";
                case 800:
                    return "Short";
                case 801:
                    return "Short";
                case 802:
                    return "Low";
                case 781:
                    return "Low";
                case 150:
                    return "Less";
                case 775:
                    return "Less Freckles";
                case 162:
                    return "Less";
                case 163:
                    return "Less";
                case 505:
                    return "Thin Lips";
                case 799:
                    return "More Upper Lip";
                case 155:
                    return "Narrow Lips";
                case 196:
                    return "Close Set Eyes";
                case 769:
                    return "Sunken Eyes";
                case 198:
                    return "Low Heels";
                case 513:
                    return "Pointy Heels";
                case 514:
                    return "Pointy";
                case 503:
                    return "Low Platforms";
                case 193:
                    return "More Square";
                case 157:
                    return "Small";
                case 637:
                    return "Less Body Fat";
                case 130:
                    return "Short";
                case 131:
                    return "Short";
                case 132:
                    return "Short";
                case 133:
                    return "Short";
                case 134:
                    return "Short";
                case 135:
                    return "Short";
                case 136:
                    return "Sweep Forward";
                case 137:
                    return "Left";
                case 608:
                    return "Short";
                case 609:
                    return "Open";
                case 105:
                    return "Small";
                case 629:
                    return "More Vertical";
                case 646:
                    return "Chin Heavy";
                case 647:
                    return "Squash Head";
                case 649:
                    return "Less Muscular";
                case 678:
                    return "Less Muscular";
                case 652:
                    return "Less Muscular";
                case 659:
                    return "Corner Down";
                case 662:
                    return "Shear Right Up";
                case 773:
                    return "Flat Head";
                case 682:
                    return "Small Head";
                case 690:
                    return "Beady Eyes";
                case 752:
                    return "5 O'Clock Shadow";
                case 763:
                    return "Less Volume";
                case 785:
                    return "Short Pigtails";
                case 789:
                    return "Short Ponytail";
                case 795:
                    return "Flat Butt";
                case 841:
                    return "Knock Kneed";
                case 753:
                    return "Less Saddle";
                case 676:
                    return "Less Love";
                case 863:
                    return "Tight Skirt";
                case 119:
                    return "Thin Eyebrows";
                case 750:
                    return "Sparse";
                case 166:
                    return "Short Sideburns";
                case 167:
                    return "Chaplin";
                case 168:
                    return "Less soul";
                case 169:
                    return "Less Curtains";
                case 606:
                    return "Short";
                case 607:
                    return "Low";
                case 780:
                    return "Low";
                case 603:
                    return "Short";
                case 604:
                    return "Short";
                case 605:
                    return "Low";
                case 779:
                    return "Low";
                case 617:
                    return "Short";
                case 616:
                    return "Short";
                case 619:
                    return "Short";
                case 624:
                    return "Low";
                case 93:
                    return "Short";
                case 844:
                    return "Fingerless";
                case 16:
                    return "Smooth";
                case 757:
                    return "Higher";
                case 31:
                    return "Flat";
                case 877:
                    return "No Wrinkles";
            }
        }
        public string GetLabelMax( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return "Male";
                case 33:
                    return "Tall";
                case 34:
                    return "Body Thick";
                case 36:
                    return "Broad";
                case 37:
                    return "Wide";
                case 842:
                    return "Long Hips";
                case 38:
                    return "Long Torso";
                case 195:
                    return "Eyes Spread";
                case 661:
                    return "Eyes Shear Right Up";
                case 772:
                    return "Eyes Long Head";
                case 768:
                    return "Eyes Bugged";
                case 655:
                    return "Big Head";
                case 197:
                    return "High Heels";
                case 502:
                    return "High Heels";
                case 675:
                    return "Large Hands";
                case 683:
                    return "Thick Neck";
                case 689:
                    return "Eyes Forward";
                case 692:
                    return "Long Legs";
                case 693:
                    return "Long arms";
                case 756:
                    return "Long Neck";
                case 180:
                    return "More";
                case 761:
                    return "More";
                case 181:
                    return "More";
                case 182:
                    return "More";
                case 183:
                    return "More";
                case 184:
                    return "Big Spikes";
                case 140:
                    return "Part";
                case 141:
                    return "Part";
                case 142:
                    return "Part";
                case 143:
                    return "Full Sides";
                case 144:
                    return "Bangs Up";
                case 145:
                    return "Bangs Down";
                case 146:
                    return "Side Bangs Up";
                case 147:
                    return "Side Bangs Down";
                case 148:
                    return "Back Bangs Up";
                case 149:
                    return "Back Bangs Down";
                case 171:
                    return "Front Hair Down";
                case 172:
                    return "Front Hair Up";
                case 173:
                    return "Sides Hair Down";
                case 174:
                    return "Sides Hair Up";
                case 175:
                    return "Back Hair Down";
                case 176:
                    return "Back Hair Up";
                case 177:
                    return "Rumpled Hair";
                case 178:
                    return "Swept Back";
                case 179:
                    return "Swept Forward";
                case 190:
                    return "Tilt Right";
                case 191:
                    return "Tilt Left";
                case 192:
                    return "Part Bangs";
                case 674:
                    return "Sheared Back";
                case 762:
                    return "Sheared Front";
                case 754:
                    return "Narrow Back";
                case 755:
                    return "Narrow Front";
                case 1:
                    return "Large";
                case 2:
                    return "Large";
                case 4:
                    return "Broad";
                case 759:
                    return "Low";
                case 517:
                    return "Wide";
                case 5:
                    return "Cleft";
                case 6:
                    return "Bulbous";
                case 7:
                    return "Chin In";
                case 8:
                    return "Double Chin";
                case 10:
                    return "Sunken";
                case 11:
                    return "High";
                case 758:
                    return "High";
                case 12:
                    return "More";
                case 13:
                    return "Cleft";
                case 14:
                    return "High";
                case 15:
                    return "Out";
                case 870:
                    return "Pointy";
                case 17:
                    return "Square";
                case 18:
                    return "Puffy";
                case 19:
                    return "Upturned";
                case 20:
                    return "Bulbous Nose";
                case 21:
                    return "Creased";
                case 22:
                    return "Attached";
                case 23:
                    return "Baggy";
                case 765:
                    return "Puffy";
                case 24:
                    return "Wide";
                case 25:
                    return "Wide";
                case 764:
                    return "Deep";
                case 27:
                    return "Wide";
                case 872:
                    return "Arced";
                case 871:
                    return "Lower";
                case 35:
                    return "Large";
                case 796:
                    return "Pointy";
                case 185:
                    return "Deep";
                case 186:
                    return "Forehead Heavy";
                case 187:
                    return "Stretch Head";
                case 188:
                    return "More Square";
                case 189:
                    return "More Round";
                case 194:
                    return "Eyes Spread";
                case 400:
                    return "Bushy Hair";
                case 506:
                    return "Low";
                case 633:
                    return "Fat";
                case 630:
                    return "More";
                case 631:
                    return "More";
                case 650:
                    return "Corner Up";
                case 880:
                    return "Corner Up";
                case 653:
                    return "More Full";
                case 656:
                    return "Nose Right";
                case 657:
                    return "Corner Up";
                case 658:
                    return "Corner Down";
                case 797:
                    return "Fat Upper";
                case 798:
                    return "Fat Lower";
                case 660:
                    return "Shear Right";
                case 770:
                    return "Long Head";
                case 663:
                    return "Shift Right";
                case 664:
                    return "Pop Left Eye";
                case 760:
                    return "High Jaw";
                case 665:
                    return "Underbite";
                case 686:
                    return "Anime Eyes";
                case 767:
                    return "Bug Eyes";
                case 518:
                    return "Long";
                case 626:
                    return "Large";
                case 627:
                    return "Small";
                case 843:
                    return "None";
                case 106:
                    return "Muscular";
                case 648:
                    return "Scrawny";
                case 677:
                    return "Scrawny";
                case 634:
                    return "fat";
                case 507:
                    return "More Gravity";
                case 840:
                    return "Loose Sleeves";
                case 684:
                    return "Join";
                case 685:
                    return "Sunken Chest";
                case 151:
                    return "Large";
                case 794:
                    return "Small";
                case 152:
                    return "More Muscles";
                case 651:
                    return "Less Muscles";
                case 500:
                    return "High Heels";
                case 501:
                    return "High Platforms";
                case 508:
                    return "Wide";
                case 509:
                    return "Pointy Heels";
                case 510:
                    return "Thick Heels";
                case 511:
                    return "Pointy Toe";
                case 512:
                    return "Square Toe";
                case 654:
                    return "Thick Toe";
                case 515:
                    return "Big";
                case 625:
                    return "Flared Cuffs";
                case 638:
                    return "Low and Loose";
                case 635:
                    return "fat";
                case 879:
                    return "Duffle Bag";
                case 679:
                    return "big eye";
                case 687:
                    return "big eye";
                case 694:
                    return "big eye";
                case 695:
                    return "big eye";
                case 680:
                    return "big eye";
                case 688:
                    return "big eye";
                case 681:
                    return "big eye";
                case 691:
                    return "big eye";
                case 845:
                    return "more poofy";
                case 846:
                    return "loose";
                case 866:
                    return "loose";
                case 867:
                    return "loose";
                case 848:
                    return "more bustle";
                case 852:
                    return "more";
                case 851:
                    return "more";
                case 856:
                    return "more";
                case 111:
                    return "Dark";
                case 110:
                    return "Ruddy";
                case 108:
                    return "Wild";
                case 114:
                    return "Blonde";
                case 113:
                    return "Very Red";
                case 115:
                    return "All White";
                case 112:
                    return "Wild";
                case 99:
                    return "Unnatural";
                case 98:
                    return "Lighter";
                case 116:
                    return "More Rosy";
                case 117:
                    return "Pinker";
                case 165:
                    return "More";
                case 700:
                    return "Black";
                case 701:
                    return "More Lipstick";
                case 702:
                    return "Glossy";
                case 704:
                    return "More Blush";
                case 705:
                    return "Orange";
                case 711:
                    return "Opaque";
                case 708:
                    return "Dark";
                case 706:
                    return "Opaque";
                case 707:
                    return "More Eyeshadow";
                case 712:
                    return "Dark";
                case 713:
                    return "Opaque";
                case 709:
                    return "More Eyeshadow";
                case 703:
                    return "Full Eyeliner";
                case 714:
                    return "Black";
                case 751:
                    return "Shadow hair";
                case 1048:
                    return "High";
                case 710:
                    return "Painted Nails";
                case 715:
                    return "Black";
                case 620:
                    return "low cut";
                case 622:
                    return "open";
                case 621:
                    return "low cut";
                case 623:
                    return "closed";
                case 858:
                    return "Long";
                case 859:
                    return "Closed Front";
                case 860:
                    return "Closed Back";
                case 861:
                    return "Closed Left";
                case 862:
                    return "Closed Right";
                case 828:
                    return "Loose Shirt";
                case 816:
                    return "Loose Pants";
                case 814:
                    return "High";
                case 815:
                    return "Long";
                case 800:
                    return "Long";
                case 801:
                    return "Long";
                case 802:
                    return "High";
                case 781:
                    return "High";
                case 150:
                    return "More";
                case 775:
                    return "More Freckles";
                case 162:
                    return "More";
                case 163:
                    return "More";
                case 505:
                    return "Fat Lips";
                case 799:
                    return "More Lower Lip";
                case 155:
                    return "Wide Lips";
                case 196:
                    return "Far Set Eyes";
                case 769:
                    return "Bugged Eyes";
                case 198:
                    return "High Heels";
                case 513:
                    return "Thick Heels";
                case 514:
                    return "Square";
                case 503:
                    return "High Platforms";
                case 193:
                    return "More Round";
                case 157:
                    return "Big";
                case 637:
                    return "More Body Fat";
                case 130:
                    return "Long";
                case 131:
                    return "Long";
                case 132:
                    return "Long";
                case 133:
                    return "Long";
                case 134:
                    return "Long";
                case 135:
                    return "Long";
                case 136:
                    return "Sweep Back";
                case 137:
                    return "Right";
                case 608:
                    return "Long";
                case 609:
                    return "Closed";
                case 105:
                    return "Large";
                case 629:
                    return "More Sloped";
                case 646:
                    return "Forehead Heavy";
                case 647:
                    return "Stretch Head";
                case 649:
                    return "More Muscular";
                case 678:
                    return "More Muscular";
                case 652:
                    return "More Muscular";
                case 659:
                    return "Corner Up";
                case 662:
                    return "Shear Left Up";
                case 773:
                    return "Long Head";
                case 682:
                    return "Big Head";
                case 690:
                    return "Anime Eyes";
                case 752:
                    return "Bushy Hair";
                case 763:
                    return "More Volume";
                case 785:
                    return "Long Pigtails";
                case 789:
                    return "Long Ponytail";
                case 795:
                    return "Big Butt";
                case 841:
                    return "Bow Legged";
                case 753:
                    return "More Saddle";
                case 676:
                    return "More Love";
                case 863:
                    return "Poofy Skirt";
                case 119:
                    return "Bushy Eyebrows";
                case 750:
                    return "Dense";
                case 166:
                    return "Mutton Chops";
                case 167:
                    return "Handlebars";
                case 168:
                    return "More soul";
                case 169:
                    return "More Curtains";
                case 606:
                    return "Long";
                case 607:
                    return "High";
                case 780:
                    return "High";
                case 603:
                    return "Long";
                case 604:
                    return "Long";
                case 605:
                    return "High";
                case 779:
                    return "High";
                case 617:
                    return "Long";
                case 616:
                    return "Tall";
                case 619:
                    return "Long";
                case 624:
                    return "High";
                case 93:
                    return "Long";
                case 844:
                    return "Fingers";
                case 16:
                    return "Pointy";
                case 757:
                    return "Lower";
                case 31:
                    return "Arced";
                case 877:
                    return "Wrinkles";
            }
        }
        public float GetValueMin( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return 0f;
                case 33:
                    return -2.3f;
                case 34:
                    return -0.7f;
                case 36:
                    return -1.8f;
                case 37:
                    return -3.2f;
                case 842:
                    return -1f;
                case 38:
                    return -1f;
                case 195:
                    return -1f;
                case 661:
                    return -2f;
                case 772:
                    return -1f;
                case 768:
                    return -2f;
                case 655:
                    return -.25f;
                case 197:
                    return 0f;
                case 502:
                    return 0f;
                case 675:
                    return -.3f;
                case 683:
                    return -.4f;
                case 689:
                    return -1f;
                case 692:
                    return -1f;
                case 693:
                    return -1f;
                case 756:
                    return -1f;
                case 180:
                    return 0f;
                case 761:
                    return 0f;
                case 181:
                    return -1f;
                case 182:
                    return -1f;
                case 183:
                    return -1f;
                case 184:
                    return 0f;
                case 140:
                    return 0f;
                case 141:
                    return 0f;
                case 142:
                    return 0f;
                case 143:
                    return -4f;
                case 144:
                    return 0f;
                case 145:
                    return 0f;
                case 146:
                    return 0f;
                case 147:
                    return 0f;
                case 148:
                    return 0f;
                case 149:
                    return 0f;
                case 171:
                    return 0f;
                case 172:
                    return 0f;
                case 173:
                    return 0f;
                case 174:
                    return 0f;
                case 175:
                    return 0f;
                case 176:
                    return 0f;
                case 177:
                    return 0f;
                case 178:
                    return 0f;
                case 179:
                    return 0f;
                case 190:
                    return 0f;
                case 191:
                    return 0f;
                case 192:
                    return 0f;
                case 674:
                    return -1f;
                case 762:
                    return 0f;
                case 754:
                    return -1f;
                case 755:
                    return -1.5f;
                case 1:
                    return -.3f;
                case 2:
                    return -0.8f;
                case 4:
                    return -.5f;
                case 759:
                    return -1f;
                case 517:
                    return -.5f;
                case 5:
                    return -.1f;
                case 6:
                    return -.3f;
                case 7:
                    return -.5f;
                case 8:
                    return -.5f;
                case 10:
                    return -1.5f;
                case 11:
                    return -.5f;
                case 758:
                    return -1.5f;
                case 12:
                    return -.5f;
                case 13:
                    return 0f;
                case 14:
                    return -.5f;
                case 15:
                    return -.5f;
                case 870:
                    return -.5f;
                case 17:
                    return -.5f;
                case 18:
                    return -1.5f;
                case 19:
                    return -1.5f;
                case 20:
                    return -.5f;
                case 21:
                    return -0.2f;
                case 22:
                    return 0f;
                case 23:
                    return -.5f;
                case 765:
                    return -.3f;
                case 24:
                    return -1.5f;
                case 25:
                    return -.8f;
                case 764:
                    return -.5f;
                case 27:
                    return -1.3f;
                case 872:
                    return 0f;
                case 871:
                    return -2f;
                case 35:
                    return -1f;
                case 796:
                    return -.4f;
                case 185:
                    return -1f;
                case 186:
                    return -1.3f;
                case 187:
                    return -.5f;
                case 188:
                    return 0f;
                case 189:
                    return 0f;
                case 194:
                    return -2f;
                case 400:
                    return 0f;
                case 506:
                    return -2f;
                case 633:
                    return 0f;
                case 630:
                    return 0f;
                case 631:
                    return 0f;
                case 650:
                    return -1.3f;
                case 880:
                    return -1.3f;
                case 653:
                    return -1f;
                case 656:
                    return -2f;
                case 657:
                    return 0f;
                case 658:
                    return 0f;
                case 797:
                    return 0f;
                case 798:
                    return 0f;
                case 660:
                    return -2f;
                case 770:
                    return -1f;
                case 663:
                    return -2f;
                case 664:
                    return -1.3f;
                case 760:
                    return -1.2f;
                case 665:
                    return -2f;
                case 686:
                    return -2f;
                case 767:
                    return -2f;
                case 518:
                    return -.3f;
                case 626:
                    return 0f;
                case 627:
                    return 0f;
                case 843:
                    return 0f;
                case 106:
                    return 0f;
                case 648:
                    return 0f;
                case 677:
                    return 0f;
                case 634:
                    return 0f;
                case 507:
                    return -1.5f;
                case 628:
                    return 0f;
                case 840:
                    return 0f;
                case 684:
                    return -.3f;
                case 685:
                    return -.5f;
                case 100:
                    return 0f;
                case 151:
                    return 0f;
                case 794:
                    return 0f;
                case 152:
                    return 0f;
                case 651:
                    return 0f;
                case 853:
                    return -1f;
                case 500:
                    return 0f;
                case 501:
                    return 0f;
                case 508:
                    return -1f;
                case 509:
                    return 0f;
                case 510:
                    return 0f;
                case 511:
                    return 0f;
                case 512:
                    return 0f;
                case 654:
                    return 0f;
                case 515:
                    return -1f;
                case 516:
                    return 0f;
                case 625:
                    return 0f;
                case 793:
                    return 0f;
                case 638:
                    return 0f;
                case 635:
                    return 0f;
                case 879:
                    return -.5f;
                case 679:
                    return -.25f;
                case 687:
                    return -.25f;
                case 694:
                    return -.25f;
                case 695:
                    return -.25f;
                case 680:
                    return -.25f;
                case 688:
                    return -.25f;
                case 681:
                    return -.25f;
                case 691:
                    return -.25f;
                case 845:
                    return 0f;
                case 846:
                    return 0f;
                case 866:
                    return 0f;
                case 867:
                    return 0f;
                case 848:
                    return 0f;
                case 847:
                    return -1f;
                case 852:
                    return 0f;
                case 849:
                    return 0f;
                case 851:
                    return 0f;
                case 856:
                    return -1f;
                case 111:
                    return 0f;
                case 110:
                    return 0f;
                case 108:
                    return 0f;
                case 114:
                    return 0f;
                case 113:
                    return 0f;
                case 115:
                    return 0f;
                case 112:
                    return 0f;
                case 99:
                    return 0f;
                case 98:
                    return 0f;
                case 116:
                    return 0f;
                case 117:
                    return 0f;
                case 165:
                    return 0f;
                case 700:
                    return 0f;
                case 701:
                    return 0f;
                case 702:
                    return 0f;
                case 704:
                    return 0f;
                case 705:
                    return 0f;
                case 711:
                    return 0f;
                case 708:
                    return 0f;
                case 706:
                    return .2f;
                case 707:
                    return 0f;
                case 712:
                    return 0f;
                case 713:
                    return .2f;
                case 709:
                    return 0f;
                case 703:
                    return 0f;
                case 714:
                    return 0f;
                case 751:
                    return 0f;
                case 1048:
                    return 0f;
                case 710:
                    return 0f;
                case 715:
                    return 0f;
                case 620:
                    return 0f;
                case 622:
                    return 0f;
                case 621:
                    return 0f;
                case 623:
                    return 0f;
                case 858:
                    return .01f;
                case 859:
                    return 0f;
                case 860:
                    return 0f;
                case 861:
                    return 0f;
                case 862:
                    return 0f;
                case 828:
                    return 0f;
                case 816:
                    return 0f;
                case 814:
                    return 0f;
                case 815:
                    return 0f;
                case 800:
                    return 0f;
                case 801:
                    return 0f;
                case 802:
                    return 0f;
                case 781:
                    return 0f;
                case 150:
                    return 0f;
                case 775:
                    return 0f;
                case 162:
                    return 0f;
                case 163:
                    return 0f;
                case 505:
                    return 0f;
                case 799:
                    return 0f;
                case 155:
                    return -0.9f;
                case 196:
                    return -2f;
                case 769:
                    return 0f;
                case 198:
                    return 0f;
                case 513:
                    return 0f;
                case 514:
                    return 0f;
                case 503:
                    return 0f;
                case 193:
                    return 0f;
                case 157:
                    return 0f;
                case 637:
                    return 0f;
                case 130:
                    return 0f;
                case 131:
                    return 0f;
                case 132:
                    return 0f;
                case 133:
                    return 0f;
                case 134:
                    return 0f;
                case 135:
                    return 0f;
                case 136:
                    return 0f;
                case 137:
                    return 0f;
                case 608:
                    return 0f;
                case 609:
                    return 0f;
                case 105:
                    return 0f;
                case 629:
                    return 0f;
                case 646:
                    return -1.3f;
                case 647:
                    return -0.5f;
                case 649:
                    return 0f;
                case 678:
                    return 0f;
                case 652:
                    return 0f;
                case 659:
                    return 0f;
                case 662:
                    return 0f;
                case 773:
                    return 0f;
                case 682:
                    return 0f;
                case 690:
                    return 0f;
                case 752:
                    return 0f;
                case 763:
                    return 0f;
                case 785:
                    return 0f;
                case 789:
                    return 0f;
                case 795:
                    return 0f;
                case 841:
                    return -1f;
                case 753:
                    return -0.5f;
                case 676:
                    return -1f;
                case 863:
                    return 0f;
                case 119:
                    return 0f;
                case 750:
                    return 0f;
                case 166:
                    return 0f;
                case 167:
                    return 0f;
                case 168:
                    return 0f;
                case 169:
                    return 0f;
                case 606:
                    return 0f;
                case 607:
                    return 0f;
                case 780:
                    return 0f;
                case 603:
                    return .01f;
                case 604:
                    return 0f;
                case 605:
                    return 0f;
                case 779:
                    return 0f;
                case 617:
                    return 0f;
                case 616:
                    return 0f;
                case 619:
                    return 0f;
                case 624:
                    return 0f;
                case 93:
                    return .01f;
                case 844:
                    return .01f;
                case 16:
                    return -.5f;
                case 757:
                    return -4f;
                case 31:
                    return 0f;
                case 877:
                    return 0f;
            }
        }
        public float GetValueMax( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return 1f;
                case 33:
                    return 2f;
                case 34:
                    return 1.5f;
                case 36:
                    return 1.4f;
                case 37:
                    return 2.8f;
                case 842:
                    return 1f;
                case 38:
                    return 1f;
                case 195:
                    return 1f;
                case 661:
                    return 2f;
                case 772:
                    return 1f;
                case 768:
                    return 2f;
                case 655:
                    return .10f;
                case 197:
                    return 1f;
                case 502:
                    return 1f;
                case 675:
                    return .3f;
                case 683:
                    return .2f;
                case 689:
                    return 1f;
                case 692:
                    return 1f;
                case 693:
                    return 1f;
                case 756:
                    return 1f;
                case 180:
                    return 1.3f;
                case 761:
                    return 1.3f;
                case 181:
                    return 1f;
                case 182:
                    return 1f;
                case 183:
                    return 1f;
                case 184:
                    return 1f;
                case 140:
                    return 2f;
                case 141:
                    return 2f;
                case 142:
                    return 2f;
                case 143:
                    return 1.5f;
                case 144:
                    return 1f;
                case 145:
                    return 5f;
                case 146:
                    return 1f;
                case 147:
                    return 2f;
                case 148:
                    return 1f;
                case 149:
                    return 2f;
                case 171:
                    return 1f;
                case 172:
                    return 1f;
                case 173:
                    return 1f;
                case 174:
                    return 1f;
                case 175:
                    return 3f;
                case 176:
                    return 1f;
                case 177:
                    return 1f;
                case 178:
                    return 1f;
                case 179:
                    return 1f;
                case 190:
                    return 1f;
                case 191:
                    return 1f;
                case 192:
                    return 1f;
                case 674:
                    return 2f;
                case 762:
                    return 3f;
                case 754:
                    return 2f;
                case 755:
                    return 1.5f;
                case 1:
                    return 2f;
                case 2:
                    return 2.5f;
                case 4:
                    return 1f;
                case 759:
                    return 1.5f;
                case 517:
                    return 1f;
                case 5:
                    return 1f;
                case 6:
                    return 1f;
                case 7:
                    return .5f;
                case 8:
                    return 1.5f;
                case 10:
                    return 3f;
                case 11:
                    return 1.5f;
                case 758:
                    return 1.5f;
                case 12:
                    return 2.5f;
                case 13:
                    return 1.5f;
                case 14:
                    return 1f;
                case 15:
                    return 1.5f;
                case 870:
                    return 1f;
                case 17:
                    return 1f;
                case 18:
                    return 2.5f;
                case 19:
                    return 1f;
                case 20:
                    return 1.5f;
                case 21:
                    return 1.3f;
                case 22:
                    return 1f;
                case 23:
                    return 1.5f;
                case 765:
                    return 2.5f;
                case 24:
                    return 2f;
                case 25:
                    return 1.5f;
                case 764:
                    return 1.2f;
                case 27:
                    return 1.2f;
                case 872:
                    return 1f;
                case 871:
                    return 2f;
                case 35:
                    return 2f;
                case 796:
                    return 3f;
                case 185:
                    return 1f;
                case 186:
                    return 1f;
                case 187:
                    return 1f;
                case 188:
                    return .7f;
                case 189:
                    return 1f;
                case 194:
                    return 2f;
                case 400:
                    return 2f;
                case 506:
                    return 2f;
                case 633:
                    return 1f;
                case 630:
                    return 1f;
                case 631:
                    return 1f;
                case 650:
                    return 1.2f;
                case 880:
                    return 1.2f;
                case 653:
                    return 2f;
                case 656:
                    return 2f;
                case 657:
                    return 1.4f;
                case 658:
                    return 1.2f;
                case 797:
                    return 1.5f;
                case 798:
                    return 1.5f;
                case 660:
                    return 2f;
                case 770:
                    return 1f;
                case 663:
                    return 2f;
                case 664:
                    return 1.3f;
                case 760:
                    return 2f;
                case 665:
                    return 2f;
                case 686:
                    return 2f;
                case 767:
                    return 2f;
                case 518:
                    return 1.5f;
                case 626:
                    return 1f;
                case 627:
                    return 1f;
                case 843:
                    return 1f;
                case 106:
                    return 1.4f;
                case 648:
                    return 1.3f;
                case 677:
                    return 1.3f;
                case 634:
                    return 1f;
                case 507:
                    return 2f;
                case 628:
                    return 1f;
                case 840:
                    return 1.5f;
                case 684:
                    return 1.3f;
                case 685:
                    return 1.1f;
                case 100:
                    return 1f;
                case 151:
                    return 1f;
                case 794:
                    return 1f;
                case 152:
                    return 1.5f;
                case 651:
                    return 1.5f;
                case 853:
                    return 1f;
                case 500:
                    return 1f;
                case 501:
                    return 1f;
                case 508:
                    return 2f;
                case 509:
                    return 1f;
                case 510:
                    return 1f;
                case 511:
                    return 1f;
                case 512:
                    return 1f;
                case 654:
                    return 2f;
                case 515:
                    return 3f;
                case 516:
                    return 1f;
                case 625:
                    return 1.5f;
                case 793:
                    return 3f;
                case 638:
                    return 1.3f;
                case 635:
                    return 1f;
                case 879:
                    return 2f;
                case 679:
                    return .10f;
                case 687:
                    return .25f;
                case 694:
                    return .10f;
                case 695:
                    return .25f;
                case 680:
                    return .10f;
                case 688:
                    return .25f;
                case 681:
                    return .10f;
                case 691:
                    return .25f;
                case 845:
                    return 1.5f;
                case 846:
                    return 1f;
                case 866:
                    return 1f;
                case 867:
                    return 1f;
                case 848:
                    return 2f;
                case 847:
                    return 1f;
                case 852:
                    return 1f;
                case 849:
                    return 1f;
                case 851:
                    return 1f;
                case 856:
                    return 2f;
                case 111:
                    return 1f;
                case 110:
                    return 0.1f;
                case 108:
                    return 1f;
                case 114:
                    return 1f;
                case 113:
                    return 1f;
                case 115:
                    return 1f;
                case 112:
                    return 1f;
                case 99:
                    return 1f;
                case 98:
                    return 1f;
                case 116:
                    return 1f;
                case 117:
                    return 1f;
                case 165:
                    return 1f;
                case 700:
                    return 1f;
                case 701:
                    return .9f;
                case 702:
                    return 1f;
                case 704:
                    return .9f;
                case 705:
                    return 1f;
                case 711:
                    return 1f;
                case 708:
                    return 1f;
                case 706:
                    return 1f;
                case 707:
                    return .7f;
                case 712:
                    return 1f;
                case 713:
                    return 1f;
                case 709:
                    return 1f;
                case 703:
                    return 1f;
                case 714:
                    return 1f;
                case 751:
                    return 1f;
                case 1048:
                    return 1f;
                case 710:
                    return 1f;
                case 715:
                    return 1f;
                case 620:
                    return 1f;
                case 622:
                    return 1f;
                case 621:
                    return 1f;
                case 623:
                    return 1f;
                case 858:
                    return 1f;
                case 859:
                    return 1f;
                case 860:
                    return 1f;
                case 861:
                    return 1f;
                case 862:
                    return 1f;
                case 828:
                    return 1f;
                case 816:
                    return 1f;
                case 814:
                    return 1f;
                case 815:
                    return 1f;
                case 800:
                    return 1f;
                case 801:
                    return 1f;
                case 802:
                    return 1f;
                case 781:
                    return 1f;
                case 150:
                    return 1f;
                case 775:
                    return 1f;
                case 162:
                    return 1f;
                case 163:
                    return 1f;
                case 505:
                    return 1f;
                case 799:
                    return 1f;
                case 155:
                    return 1.3f;
                case 196:
                    return 1f;
                case 769:
                    return 1f;
                case 198:
                    return 1f;
                case 513:
                    return 1f;
                case 514:
                    return 1f;
                case 503:
                    return 1f;
                case 193:
                    return 1f;
                case 157:
                    return 1f;
                case 637:
                    return 1f;
                case 130:
                    return 1f;
                case 131:
                    return 1f;
                case 132:
                    return 1f;
                case 133:
                    return 1f;
                case 134:
                    return 1f;
                case 135:
                    return 1f;
                case 136:
                    return 1f;
                case 137:
                    return 1f;
                case 608:
                    return 1f;
                case 609:
                    return 1f;
                case 105:
                    return 1f;
                case 629:
                    return 1f;
                case 646:
                    return 1f;
                case 647:
                    return 1f;
                case 649:
                    return 1f;
                case 678:
                    return 1f;
                case 652:
                    return 1f;
                case 659:
                    return 1f;
                case 662:
                    return 1f;
                case 773:
                    return 1f;
                case 682:
                    return 1f;
                case 690:
                    return 1f;
                case 752:
                    return 1f;
                case 763:
                    return 1f;
                case 785:
                    return 1f;
                case 789:
                    return 1f;
                case 795:
                    return 1f;
                case 841:
                    return 1f;
                case 753:
                    return 3f;
                case 676:
                    return 2f;
                case 863:
                    return 1f;
                case 119:
                    return 1f;
                case 750:
                    return 1f;
                case 166:
                    return 1f;
                case 167:
                    return 1f;
                case 168:
                    return 1f;
                case 169:
                    return 1f;
                case 606:
                    return 1f;
                case 607:
                    return 1f;
                case 780:
                    return 1f;
                case 603:
                    return 1f;
                case 604:
                    return 1f;
                case 605:
                    return 1f;
                case 779:
                    return 1f;
                case 617:
                    return 1f;
                case 616:
                    return 1f;
                case 619:
                    return 1f;
                case 624:
                    return 1f;
                case 93:
                    return 1f;
                case 844:
                    return 1f;
                case 16:
                    return 3f;
                case 757:
                    return 2f;
                case 31:
                    return 2f;
                case 877:
                    return 1f;
            }
        }
        public float GetValueDefault( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return 0f;
                case 33:
                    return -2.3f;
                case 34:
                    return -0.7f;
                case 36:
                    return -0.5f;
                case 37:
                    return -3.2f;
                case 842:
                    return -1f;
                case 38:
                    return -1f;
                case 195:
                    return -1f;
                case 661:
                    return -2f;
                case 772:
                    return -1f;
                case 768:
                    return -2f;
                case 655:
                    return -.25f;
                case 197:
                    return 0f;
                case 502:
                    return 0f;
                case 675:
                    return -.3f;
                case 683:
                    return -.15f;
                case 689:
                    return -1f;
                case 692:
                    return -1f;
                case 693:
                    return .6f;
                case 756:
                    return 0f;
                case 180:
                    return 0f;
                case 761:
                    return 0f;
                case 181:
                    return 0.14f;
                case 182:
                    return .7f;
                case 183:
                    return 0.05f;
                case 184:
                    return 0f;
                case 140:
                    return 0f;
                case 141:
                    return 0f;
                case 142:
                    return 0f;
                case 143:
                    return 0.125f;
                case 144:
                    return 0f;
                case 145:
                    return 0f;
                case 146:
                    return 0f;
                case 147:
                    return 0f;
                case 148:
                    return 0f;
                case 149:
                    return 0f;
                case 171:
                    return 0f;
                case 172:
                    return 0f;
                case 173:
                    return 0f;
                case 174:
                    return 0f;
                case 175:
                    return 0f;
                case 176:
                    return 0f;
                case 177:
                    return 0f;
                case 178:
                    return 0f;
                case 179:
                    return 0f;
                case 190:
                    return 0f;
                case 191:
                    return 0f;
                case 192:
                    return 0f;
                case 674:
                    return -0.3f;
                case 762:
                    return 0f;
                case 754:
                    return 0f;
                case 755:
                    return 0.05f;
                case 1:
                    return -.3f;
                case 2:
                    return -0.8f;
                case 4:
                    return -.5f;
                case 759:
                    return 0.5f;
                case 517:
                    return -.5f;
                case 5:
                    return -.1f;
                case 6:
                    return -.3f;
                case 7:
                    return -.5f;
                case 8:
                    return -.5f;
                case 10:
                    return -1.5f;
                case 11:
                    return -.5f;
                case 758:
                    return -1.5f;
                case 12:
                    return -.5f;
                case 13:
                    return 0f;
                case 14:
                    return -.5f;
                case 15:
                    return -.5f;
                case 870:
                    return -.5f;
                case 17:
                    return -.5f;
                case 18:
                    return -1.5f;
                case 19:
                    return -1.5f;
                case 20:
                    return -.5f;
                case 21:
                    return -0.2f;
                case 22:
                    return 0f;
                case 23:
                    return -.5f;
                case 765:
                    return -.3f;
                case 24:
                    return -1.5f;
                case 25:
                    return -.8f;
                case 764:
                    return -.5f;
                case 27:
                    return -1.3f;
                case 872:
                    return 0f;
                case 871:
                    return -2f;
                case 35:
                    return -1f;
                case 796:
                    return -.4f;
                case 185:
                    return -1f;
                case 186:
                    return -1.3f;
                case 187:
                    return -.5f;
                case 188:
                    return 0f;
                case 189:
                    return 0f;
                case 194:
                    return -2f;
                case 400:
                    return 0f;
                case 506:
                    return -2f;
                case 633:
                    return 0f;
                case 630:
                    return 0f;
                case 631:
                    return 0f;
                case 650:
                    return -1.3f;
                case 880:
                    return -1.3f;
                case 653:
                    return -1f;
                case 656:
                    return -2f;
                case 657:
                    return 0f;
                case 658:
                    return 0f;
                case 797:
                    return 0f;
                case 798:
                    return 0f;
                case 660:
                    return 0f;
                case 770:
                    return 0f;
                case 663:
                    return 0f;
                case 664:
                    return 0f;
                case 760:
                    return 0f;
                case 665:
                    return 0f;
                case 686:
                    return 0f;
                case 767:
                    return 0f;
                case 518:
                    return -.3f;
                case 626:
                    return 0f;
                case 627:
                    return 0f;
                case 843:
                    return 0f;
                case 106:
                    return 0f;
                case 648:
                    return 0f;
                case 677:
                    return 0f;
                case 634:
                    return 0f;
                case 507:
                    return 0f;
                case 628:
                    return 0f;
                case 840:
                    return 0f;
                case 684:
                    return 0f;
                case 685:
                    return 0f;
                case 100:
                    return 0f;
                case 151:
                    return 0f;
                case 794:
                    return 0f;
                case 152:
                    return 0f;
                case 651:
                    return 0f;
                case 853:
                    return -1f;
                case 500:
                    return 0f;
                case 501:
                    return 0f;
                case 508:
                    return -1f;
                case 509:
                    return 0f;
                case 510:
                    return 0f;
                case 511:
                    return 0f;
                case 512:
                    return 0f;
                case 654:
                    return 0f;
                case 515:
                    return -1f;
                case 516:
                    return 0f;
                case 625:
                    return 0f;
                case 793:
                    return 0f;
                case 638:
                    return 0f;
                case 635:
                    return 0f;
                case 879:
                    return 0f;
                case 679:
                    return -.25f;
                case 687:
                    return -.25f;
                case 694:
                    return -.25f;
                case 695:
                    return -.25f;
                case 680:
                    return -.25f;
                case 688:
                    return -.25f;
                case 681:
                    return -.25f;
                case 691:
                    return -.25f;
                case 845:
                    return 0f;
                case 846:
                    return 0f;
                case 866:
                    return 0f;
                case 867:
                    return 0f;
                case 848:
                    return .2f;
                case 847:
                    return 0f;
                case 852:
                    return 0f;
                case 849:
                    return 0f;
                case 851:
                    return 0f;
                case 856:
                    return 0f;
                case 111:
                    return .5f;
                case 110:
                    return 0f;
                case 108:
                    return 0f;
                case 114:
                    return .5f;
                case 113:
                    return 0f;
                case 115:
                    return 0f;
                case 112:
                    return 0f;
                case 99:
                    return 0f;
                case 98:
                    return 0f;
                case 116:
                    return 0f;
                case 117:
                    return 0f;
                case 165:
                    return 0f;
                case 700:
                    return .25f;
                case 701:
                    return 0.0f;
                case 702:
                    return 0f;
                case 704:
                    return 0f;
                case 705:
                    return .5f;
                case 711:
                    return .5f;
                case 708:
                    return 0f;
                case 706:
                    return .6f;
                case 707:
                    return 0f;
                case 712:
                    return 0f;
                case 713:
                    return .7f;
                case 709:
                    return 0f;
                case 703:
                    return 0.0f;
                case 714:
                    return 0f;
                case 751:
                    return 0.7f;
                case 1048:
                    return .8f;
                case 710:
                    return 0.0f;
                case 715:
                    return 0f;
                case 620:
                    return .8f;
                case 622:
                    return .8f;
                case 621:
                    return .8f;
                case 623:
                    return .8f;
                case 858:
                    return .4f;
                case 859:
                    return 1f;
                case 860:
                    return 1f;
                case 861:
                    return 1f;
                case 862:
                    return 1f;
                case 828:
                    return 0f;
                case 816:
                    return 0f;
                case 814:
                    return 1f;
                case 815:
                    return .8f;
                case 800:
                    return .89f;
                case 801:
                    return 1f;
                case 802:
                    return .78f;
                case 781:
                    return .78f;
                case 150:
                    return 0f;
                case 775:
                    return 0f;
                case 162:
                    return 0f;
                case 163:
                    return 0f;
                case 505:
                    return .5f;
                case 799:
                    return .5f;
                case 155:
                    return 0f;
                case 196:
                    return 0f;
                case 769:
                    return .5f;
                case 198:
                    return 0f;
                case 513:
                    return .5f;
                case 514:
                    return .5f;
                case 503:
                    return 0f;
                case 193:
                    return .5f;
                case 157:
                    return 0f;
                case 637:
                    return 0f;
                case 130:
                    return .45f;
                case 131:
                    return .5f;
                case 132:
                    return .39f;
                case 133:
                    return .25f;
                case 134:
                    return .5f;
                case 135:
                    return .55f;
                case 136:
                    return .5f;
                case 137:
                    return .5f;
                case 608:
                    return .8f;
                case 609:
                    return .2f;
                case 105:
                    return .5f;
                case 629:
                    return .5f;
                case 646:
                    return 0f;
                case 647:
                    return 0f;
                case 649:
                    return .5f;
                case 678:
                    return .5f;
                case 652:
                    return .5f;
                case 659:
                    return .5f;
                case 662:
                    return .5f;
                case 773:
                    return .5f;
                case 682:
                    return .5f;
                case 690:
                    return .5f;
                case 752:
                    return .5f;
                case 763:
                    return .55f;
                case 785:
                    return 0f;
                case 789:
                    return 0f;
                case 795:
                    return .25f;
                case 841:
                    return 0f;
                case 753:
                    return 0f;
                case 676:
                    return 0f;
                case 863:
                    return .333f;
                case 119:
                    return 0.5f;
                case 750:
                    return 0.7f;
                case 166:
                    return 0.0f;
                case 167:
                    return 0.0f;
                case 168:
                    return 0.0f;
                case 169:
                    return 0.0f;
                case 606:
                    return .8f;
                case 607:
                    return .8f;
                case 780:
                    return .8f;
                case 603:
                    return .4f;
                case 604:
                    return .85f;
                case 605:
                    return .84f;
                case 779:
                    return .84f;
                case 617:
                    return 0.35f;
                case 616:
                    return 0.1f;
                case 619:
                    return .3f;
                case 624:
                    return .8f;
                case 93:
                    return .8f;
                case 844:
                    return 1f;
                case 16:
                    return -.5f;
                case 757:
                    return -1f;
                case 31:
                    return .5f;
                case 877:
                    return 0f;
            }
        }
        public bool IsValueValid( uint Param, float Value )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 32:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 33:
                    return ( (Value >= -2.3f) && (Value <= 2f) );
                case 34:
                    return ( (Value >= -0.7f) && (Value <= 1.5f) );
                case 36:
                    return ( (Value >= -1.8f) && (Value <= 1.4f) );
                case 37:
                    return ( (Value >= -3.2f) && (Value <= 2.8f) );
                case 842:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 38:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 195:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 661:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 772:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 768:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 655:
                    return ( (Value >= -.25f) && (Value <= .10f) );
                case 197:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 502:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 675:
                    return ( (Value >= -.3f) && (Value <= .3f) );
                case 683:
                    return ( (Value >= -.4f) && (Value <= .2f) );
                case 689:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 692:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 693:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 756:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 180:
                    return ( (Value >= 0f) && (Value <= 1.3f) );
                case 761:
                    return ( (Value >= 0f) && (Value <= 1.3f) );
                case 181:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 182:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 183:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 184:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 140:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 141:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 142:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 143:
                    return ( (Value >= -4f) && (Value <= 1.5f) );
                case 144:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 145:
                    return ( (Value >= 0f) && (Value <= 5f) );
                case 146:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 147:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 148:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 149:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 171:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 172:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 173:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 174:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 175:
                    return ( (Value >= 0f) && (Value <= 3f) );
                case 176:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 177:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 178:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 179:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 190:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 191:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 192:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 674:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 762:
                    return ( (Value >= 0f) && (Value <= 3f) );
                case 754:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 755:
                    return ( (Value >= -1.5f) && (Value <= 1.5f) );
                case 1:
                    return ( (Value >= -.3f) && (Value <= 2f) );
                case 2:
                    return ( (Value >= -0.8f) && (Value <= 2.5f) );
                case 4:
                    return ( (Value >= -.5f) && (Value <= 1f) );
                case 759:
                    return ( (Value >= -1f) && (Value <= 1.5f) );
                case 517:
                    return ( (Value >= -.5f) && (Value <= 1f) );
                case 5:
                    return ( (Value >= -.1f) && (Value <= 1f) );
                case 6:
                    return ( (Value >= -.3f) && (Value <= 1f) );
                case 7:
                    return ( (Value >= -.5f) && (Value <= .5f) );
                case 8:
                    return ( (Value >= -.5f) && (Value <= 1.5f) );
                case 10:
                    return ( (Value >= -1.5f) && (Value <= 3f) );
                case 11:
                    return ( (Value >= -.5f) && (Value <= 1.5f) );
                case 758:
                    return ( (Value >= -1.5f) && (Value <= 1.5f) );
                case 12:
                    return ( (Value >= -.5f) && (Value <= 2.5f) );
                case 13:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 14:
                    return ( (Value >= -.5f) && (Value <= 1f) );
                case 15:
                    return ( (Value >= -.5f) && (Value <= 1.5f) );
                case 870:
                    return ( (Value >= -.5f) && (Value <= 1f) );
                case 17:
                    return ( (Value >= -.5f) && (Value <= 1f) );
                case 18:
                    return ( (Value >= -1.5f) && (Value <= 2.5f) );
                case 19:
                    return ( (Value >= -1.5f) && (Value <= 1f) );
                case 20:
                    return ( (Value >= -.5f) && (Value <= 1.5f) );
                case 21:
                    return ( (Value >= -0.2f) && (Value <= 1.3f) );
                case 22:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 23:
                    return ( (Value >= -.5f) && (Value <= 1.5f) );
                case 765:
                    return ( (Value >= -.3f) && (Value <= 2.5f) );
                case 24:
                    return ( (Value >= -1.5f) && (Value <= 2f) );
                case 25:
                    return ( (Value >= -.8f) && (Value <= 1.5f) );
                case 764:
                    return ( (Value >= -.5f) && (Value <= 1.2f) );
                case 27:
                    return ( (Value >= -1.3f) && (Value <= 1.2f) );
                case 872:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 871:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 35:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 796:
                    return ( (Value >= -.4f) && (Value <= 3f) );
                case 185:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 186:
                    return ( (Value >= -1.3f) && (Value <= 1f) );
                case 187:
                    return ( (Value >= -.5f) && (Value <= 1f) );
                case 188:
                    return ( (Value >= 0f) && (Value <= .7f) );
                case 189:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 194:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 400:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 506:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 633:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 630:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 631:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 650:
                    return ( (Value >= -1.3f) && (Value <= 1.2f) );
                case 880:
                    return ( (Value >= -1.3f) && (Value <= 1.2f) );
                case 653:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 656:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 657:
                    return ( (Value >= 0f) && (Value <= 1.4f) );
                case 658:
                    return ( (Value >= 0f) && (Value <= 1.2f) );
                case 797:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 798:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 660:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 770:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 663:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 664:
                    return ( (Value >= -1.3f) && (Value <= 1.3f) );
                case 760:
                    return ( (Value >= -1.2f) && (Value <= 2f) );
                case 665:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 686:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 767:
                    return ( (Value >= -2f) && (Value <= 2f) );
                case 518:
                    return ( (Value >= -.3f) && (Value <= 1.5f) );
                case 626:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 627:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 843:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 106:
                    return ( (Value >= 0f) && (Value <= 1.4f) );
                case 648:
                    return ( (Value >= 0f) && (Value <= 1.3f) );
                case 677:
                    return ( (Value >= 0f) && (Value <= 1.3f) );
                case 634:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 507:
                    return ( (Value >= -1.5f) && (Value <= 2f) );
                case 628:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 840:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 684:
                    return ( (Value >= -.3f) && (Value <= 1.3f) );
                case 685:
                    return ( (Value >= -.5f) && (Value <= 1.1f) );
                case 100:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 151:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 794:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 152:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 651:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 853:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 500:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 501:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 508:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 509:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 510:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 511:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 512:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 654:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 515:
                    return ( (Value >= -1f) && (Value <= 3f) );
                case 516:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 625:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 793:
                    return ( (Value >= 0f) && (Value <= 3f) );
                case 638:
                    return ( (Value >= 0f) && (Value <= 1.3f) );
                case 635:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 879:
                    return ( (Value >= -.5f) && (Value <= 2f) );
                case 679:
                    return ( (Value >= -.25f) && (Value <= .10f) );
                case 687:
                    return ( (Value >= -.25f) && (Value <= .25f) );
                case 694:
                    return ( (Value >= -.25f) && (Value <= .10f) );
                case 695:
                    return ( (Value >= -.25f) && (Value <= .25f) );
                case 680:
                    return ( (Value >= -.25f) && (Value <= .10f) );
                case 688:
                    return ( (Value >= -.25f) && (Value <= .25f) );
                case 681:
                    return ( (Value >= -.25f) && (Value <= .10f) );
                case 691:
                    return ( (Value >= -.25f) && (Value <= .25f) );
                case 845:
                    return ( (Value >= 0f) && (Value <= 1.5f) );
                case 846:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 866:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 867:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 848:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 847:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 852:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 849:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 851:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 856:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 111:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 110:
                    return ( (Value >= 0f) && (Value <= 0.1f) );
                case 108:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 114:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 113:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 115:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 112:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 99:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 98:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 116:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 117:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 165:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 700:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 701:
                    return ( (Value >= 0f) && (Value <= .9f) );
                case 702:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 704:
                    return ( (Value >= 0f) && (Value <= .9f) );
                case 705:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 711:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 708:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 706:
                    return ( (Value >= .2f) && (Value <= 1f) );
                case 707:
                    return ( (Value >= 0f) && (Value <= .7f) );
                case 712:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 713:
                    return ( (Value >= .2f) && (Value <= 1f) );
                case 709:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 703:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 714:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 751:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 1048:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 710:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 715:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 620:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 622:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 621:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 623:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 858:
                    return ( (Value >= .01f) && (Value <= 1f) );
                case 859:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 860:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 861:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 862:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 828:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 816:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 814:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 815:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 800:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 801:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 802:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 781:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 150:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 775:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 162:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 163:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 505:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 799:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 155:
                    return ( (Value >= -0.9f) && (Value <= 1.3f) );
                case 196:
                    return ( (Value >= -2f) && (Value <= 1f) );
                case 769:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 198:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 513:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 514:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 503:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 193:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 157:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 637:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 130:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 131:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 132:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 133:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 134:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 135:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 136:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 137:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 608:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 609:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 105:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 629:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 646:
                    return ( (Value >= -1.3f) && (Value <= 1f) );
                case 647:
                    return ( (Value >= -0.5f) && (Value <= 1f) );
                case 649:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 678:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 652:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 659:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 662:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 773:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 682:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 690:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 752:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 763:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 785:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 789:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 795:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 841:
                    return ( (Value >= -1f) && (Value <= 1f) );
                case 753:
                    return ( (Value >= -0.5f) && (Value <= 3f) );
                case 676:
                    return ( (Value >= -1f) && (Value <= 2f) );
                case 863:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 119:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 750:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 166:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 167:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 168:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 169:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 606:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 607:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 780:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 603:
                    return ( (Value >= .01f) && (Value <= 1f) );
                case 604:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 605:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 779:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 617:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 616:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 619:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 624:
                    return ( (Value >= 0f) && (Value <= 1f) );
                case 93:
                    return ( (Value >= .01f) && (Value <= 1f) );
                case 844:
                    return ( (Value >= .01f) && (Value <= 1f) );
                case 16:
                    return ( (Value >= -.5f) && (Value <= 3f) );
                case 757:
                    return ( (Value >= -4f) && (Value <= 2f) );
                case 31:
                    return ( (Value >= 0f) && (Value <= 2f) );
                case 877:
                    return ( (Value >= 0f) && (Value <= 1f) );
            }
        }
        public bool IsValid( Dictionary<uint,float> BodyShape )
        {
            foreach(KeyValuePair<uint, float> kvp in BodyShape)
            {
                if( !IsValueValid(kvp.Key, kvp.Value) ) { return false; }
            }

            return true;
        }
        public string ToString( Dictionary<uint,float> BodyShape )
        {
            StringWriter sw = new StringWriter();

            foreach(KeyValuePair<uint, float> kvp in BodyShape)
            {
                sw.Write( kvp.Key + ":" );
                sw.Write( GetLabel(kvp.Key) + ":" );
                sw.WriteLine( kvp.Value );
            }

            return sw.ToString();
        }
    }
}
