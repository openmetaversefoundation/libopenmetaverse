using System;
using libsecondlife.AssetSystem.BodyShape;

namespace libsecondlife.AssetSystem.BodyShape
{
    class BodyShapeParams
    {
        public string GetLabel( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
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
        public string GetName( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 33:
                    return "Height";
                case 34:
                    return "Thickness";
                case 36:
                    return "Shoulders";
                case 37:
                    return "Hip Width";
                case 655:
                    return "Head Size";
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
                case 110:
                    return "Red Skin";
                case 828:
                    return "Loose Upper Clothing";
                case 816:
                    return "Loose Lower Clothing";
                case 799:
                    return "Lip Ratio";
                case 155:
                    return "Lip Width";
                case 196:
                    return "Eye Spacing";
                case 193:
                    return "Head Shape";
                case 608:
                    return "bottom length lower";
                case 609:
                    return "open jacket";
                case 646:
                    return "Egg_Head";
                case 647:
                    return "Squash_Stretch_Head";
                case 649:
                    return "Torso Muscles";
                case 682:
                    return "Head Size";
                case 690:
                    return "Eye Size";
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
        public float GetValueMin( uint Param )
        {
            switch( Param )
            {
                default:
                    throw new Exception("Unknown Body Part Parameter: " + Param);
                case 33:
                    return -2.3f;
                case 34:
                    return -0.7f;
                case 36:
                    return -1.8f;
                case 37:
                    return -3.2f;
                case 655:
                    return -.25f;
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
                case 110:
                    return 0f;
                case 828:
                    return 0f;
                case 816:
                    return 0f;
                case 799:
                    return 0f;
                case 155:
                    return -0.9f;
                case 196:
                    return -2f;
                case 193:
                    return 0f;
                case 608:
                    return 0f;
                case 609:
                    return 0f;
                case 646:
                    return -1.3f;
                case 647:
                    return -0.5f;
                case 649:
                    return 0f;
                case 682:
                    return 0f;
                case 690:
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
                case 33:
                    return 2f;
                case 34:
                    return 1.5f;
                case 36:
                    return 1.4f;
                case 37:
                    return 2.8f;
                case 655:
                    return .10f;
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
                case 110:
                    return 0.1f;
                case 828:
                    return 1f;
                case 816:
                    return 1f;
                case 799:
                    return 1f;
                case 155:
                    return 1.3f;
                case 196:
                    return 1f;
                case 193:
                    return 1f;
                case 608:
                    return 1f;
                case 609:
                    return 1f;
                case 646:
                    return 1f;
                case 647:
                    return 1f;
                case 649:
                    return 1f;
                case 682:
                    return 1f;
                case 690:
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
                case 33:
                    return -2.3f;
                case 34:
                    return -0.7f;
                case 36:
                    return -0.5f;
                case 37:
                    return -3.2f;
                case 655:
                    return -.25f;
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
                case 110:
                    return 0f;
                case 828:
                    return 0f;
                case 816:
                    return 0f;
                case 799:
                    return .5f;
                case 155:
                    return 0f;
                case 196:
                    return 0f;
                case 193:
                    return .5f;
                case 608:
                    return .8f;
                case 609:
                    return .2f;
                case 646:
                    return 0f;
                case 647:
                    return 0f;
                case 649:
                    return .5f;
                case 682:
                    return .5f;
                case 690:
                    return .5f;
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
                case 33:
                    return ( (Value > -2.3f) && (Value < 2f) );
                case 34:
                    return ( (Value > -0.7f) && (Value < 1.5f) );
                case 36:
                    return ( (Value > -1.8f) && (Value < 1.4f) );
                case 37:
                    return ( (Value > -3.2f) && (Value < 2.8f) );
                case 655:
                    return ( (Value > -.25f) && (Value < .10f) );
                case 180:
                    return ( (Value > 0f) && (Value < 1.3f) );
                case 761:
                    return ( (Value > 0f) && (Value < 1.3f) );
                case 181:
                    return ( (Value > -1f) && (Value < 1f) );
                case 182:
                    return ( (Value > -1f) && (Value < 1f) );
                case 183:
                    return ( (Value > -1f) && (Value < 1f) );
                case 184:
                    return ( (Value > 0f) && (Value < 1f) );
                case 140:
                    return ( (Value > 0f) && (Value < 2f) );
                case 141:
                    return ( (Value > 0f) && (Value < 2f) );
                case 142:
                    return ( (Value > 0f) && (Value < 2f) );
                case 143:
                    return ( (Value > -4f) && (Value < 1.5f) );
                case 144:
                    return ( (Value > 0f) && (Value < 1f) );
                case 145:
                    return ( (Value > 0f) && (Value < 5f) );
                case 146:
                    return ( (Value > 0f) && (Value < 1f) );
                case 147:
                    return ( (Value > 0f) && (Value < 2f) );
                case 148:
                    return ( (Value > 0f) && (Value < 1f) );
                case 149:
                    return ( (Value > 0f) && (Value < 2f) );
                case 171:
                    return ( (Value > 0f) && (Value < 1f) );
                case 172:
                    return ( (Value > 0f) && (Value < 1f) );
                case 173:
                    return ( (Value > 0f) && (Value < 1f) );
                case 174:
                    return ( (Value > 0f) && (Value < 1f) );
                case 175:
                    return ( (Value > 0f) && (Value < 3f) );
                case 176:
                    return ( (Value > 0f) && (Value < 1f) );
                case 177:
                    return ( (Value > 0f) && (Value < 1f) );
                case 178:
                    return ( (Value > 0f) && (Value < 1f) );
                case 179:
                    return ( (Value > 0f) && (Value < 1f) );
                case 190:
                    return ( (Value > 0f) && (Value < 1f) );
                case 191:
                    return ( (Value > 0f) && (Value < 1f) );
                case 192:
                    return ( (Value > 0f) && (Value < 1f) );
                case 674:
                    return ( (Value > -1f) && (Value < 2f) );
                case 762:
                    return ( (Value > 0f) && (Value < 3f) );
                case 754:
                    return ( (Value > -1f) && (Value < 2f) );
                case 755:
                    return ( (Value > -1.5f) && (Value < 1.5f) );
                case 1:
                    return ( (Value > -.3f) && (Value < 2f) );
                case 2:
                    return ( (Value > -0.8f) && (Value < 2.5f) );
                case 4:
                    return ( (Value > -.5f) && (Value < 1f) );
                case 759:
                    return ( (Value > -1f) && (Value < 1.5f) );
                case 517:
                    return ( (Value > -.5f) && (Value < 1f) );
                case 5:
                    return ( (Value > -.1f) && (Value < 1f) );
                case 6:
                    return ( (Value > -.3f) && (Value < 1f) );
                case 7:
                    return ( (Value > -.5f) && (Value < .5f) );
                case 8:
                    return ( (Value > -.5f) && (Value < 1.5f) );
                case 10:
                    return ( (Value > -1.5f) && (Value < 3f) );
                case 11:
                    return ( (Value > -.5f) && (Value < 1.5f) );
                case 758:
                    return ( (Value > -1.5f) && (Value < 1.5f) );
                case 13:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 14:
                    return ( (Value > -.5f) && (Value < 1f) );
                case 15:
                    return ( (Value > -.5f) && (Value < 1.5f) );
                case 870:
                    return ( (Value > -.5f) && (Value < 1f) );
                case 17:
                    return ( (Value > -.5f) && (Value < 1f) );
                case 18:
                    return ( (Value > -1.5f) && (Value < 2.5f) );
                case 19:
                    return ( (Value > -1.5f) && (Value < 1f) );
                case 20:
                    return ( (Value > -.5f) && (Value < 1.5f) );
                case 21:
                    return ( (Value > -0.2f) && (Value < 1.3f) );
                case 22:
                    return ( (Value > 0f) && (Value < 1f) );
                case 23:
                    return ( (Value > -.5f) && (Value < 1.5f) );
                case 765:
                    return ( (Value > -.3f) && (Value < 2.5f) );
                case 24:
                    return ( (Value > -1.5f) && (Value < 2f) );
                case 25:
                    return ( (Value > -.8f) && (Value < 1.5f) );
                case 764:
                    return ( (Value > -.5f) && (Value < 1.2f) );
                case 27:
                    return ( (Value > -1.3f) && (Value < 1.2f) );
                case 872:
                    return ( (Value > 0f) && (Value < 1f) );
                case 871:
                    return ( (Value > -2f) && (Value < 2f) );
                case 35:
                    return ( (Value > -1f) && (Value < 2f) );
                case 796:
                    return ( (Value > -.4f) && (Value < 3f) );
                case 185:
                    return ( (Value > -1f) && (Value < 1f) );
                case 186:
                    return ( (Value > -1.3f) && (Value < 1f) );
                case 187:
                    return ( (Value > -.5f) && (Value < 1f) );
                case 400:
                    return ( (Value > 0f) && (Value < 2f) );
                case 506:
                    return ( (Value > -2f) && (Value < 2f) );
                case 633:
                    return ( (Value > 0f) && (Value < 1f) );
                case 630:
                    return ( (Value > 0f) && (Value < 1f) );
                case 631:
                    return ( (Value > 0f) && (Value < 1f) );
                case 650:
                    return ( (Value > -1.3f) && (Value < 1.2f) );
                case 880:
                    return ( (Value > -1.3f) && (Value < 1.2f) );
                case 653:
                    return ( (Value > -1f) && (Value < 2f) );
                case 656:
                    return ( (Value > -2f) && (Value < 2f) );
                case 657:
                    return ( (Value > 0f) && (Value < 1.4f) );
                case 658:
                    return ( (Value > 0f) && (Value < 1.2f) );
                case 797:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 798:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 660:
                    return ( (Value > -2f) && (Value < 2f) );
                case 770:
                    return ( (Value > -1f) && (Value < 1f) );
                case 663:
                    return ( (Value > -2f) && (Value < 2f) );
                case 664:
                    return ( (Value > -1.3f) && (Value < 1.3f) );
                case 760:
                    return ( (Value > -1.2f) && (Value < 2f) );
                case 665:
                    return ( (Value > -2f) && (Value < 2f) );
                case 686:
                    return ( (Value > -2f) && (Value < 2f) );
                case 767:
                    return ( (Value > -2f) && (Value < 2f) );
                case 518:
                    return ( (Value > -.3f) && (Value < 1.5f) );
                case 626:
                    return ( (Value > 0f) && (Value < 1f) );
                case 627:
                    return ( (Value > 0f) && (Value < 1f) );
                case 843:
                    return ( (Value > 0f) && (Value < 1f) );
                case 106:
                    return ( (Value > 0f) && (Value < 1.4f) );
                case 648:
                    return ( (Value > 0f) && (Value < 1.3f) );
                case 677:
                    return ( (Value > 0f) && (Value < 1.3f) );
                case 634:
                    return ( (Value > 0f) && (Value < 1f) );
                case 507:
                    return ( (Value > -1.5f) && (Value < 2f) );
                case 628:
                    return ( (Value > 0f) && (Value < 1f) );
                case 840:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 684:
                    return ( (Value > -.3f) && (Value < 1.3f) );
                case 685:
                    return ( (Value > -.5f) && (Value < 1.1f) );
                case 151:
                    return ( (Value > 0f) && (Value < 1f) );
                case 794:
                    return ( (Value > 0f) && (Value < 1f) );
                case 152:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 651:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 853:
                    return ( (Value > -1f) && (Value < 1f) );
                case 500:
                    return ( (Value > 0f) && (Value < 1f) );
                case 501:
                    return ( (Value > 0f) && (Value < 1f) );
                case 508:
                    return ( (Value > -1f) && (Value < 2f) );
                case 509:
                    return ( (Value > 0f) && (Value < 1f) );
                case 510:
                    return ( (Value > 0f) && (Value < 1f) );
                case 511:
                    return ( (Value > 0f) && (Value < 1f) );
                case 512:
                    return ( (Value > 0f) && (Value < 1f) );
                case 654:
                    return ( (Value > 0f) && (Value < 2f) );
                case 515:
                    return ( (Value > -1f) && (Value < 3f) );
                case 516:
                    return ( (Value > 0f) && (Value < 1f) );
                case 625:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 793:
                    return ( (Value > 0f) && (Value < 3f) );
                case 638:
                    return ( (Value > 0f) && (Value < 1.3f) );
                case 635:
                    return ( (Value > 0f) && (Value < 1f) );
                case 879:
                    return ( (Value > -.5f) && (Value < 2f) );
                case 679:
                    return ( (Value > -.25f) && (Value < .10f) );
                case 687:
                    return ( (Value > -.25f) && (Value < .25f) );
                case 694:
                    return ( (Value > -.25f) && (Value < .10f) );
                case 695:
                    return ( (Value > -.25f) && (Value < .25f) );
                case 680:
                    return ( (Value > -.25f) && (Value < .10f) );
                case 688:
                    return ( (Value > -.25f) && (Value < .25f) );
                case 681:
                    return ( (Value > -.25f) && (Value < .10f) );
                case 691:
                    return ( (Value > -.25f) && (Value < .25f) );
                case 845:
                    return ( (Value > 0f) && (Value < 1.5f) );
                case 846:
                    return ( (Value > 0f) && (Value < 1f) );
                case 866:
                    return ( (Value > 0f) && (Value < 1f) );
                case 867:
                    return ( (Value > 0f) && (Value < 1f) );
                case 848:
                    return ( (Value > 0f) && (Value < 2f) );
                case 847:
                    return ( (Value > -1f) && (Value < 1f) );
                case 852:
                    return ( (Value > 0f) && (Value < 1f) );
                case 849:
                    return ( (Value > 0f) && (Value < 1f) );
                case 110:
                    return ( (Value > 0f) && (Value < 0.1f) );
                case 828:
                    return ( (Value > 0f) && (Value < 1f) );
                case 816:
                    return ( (Value > 0f) && (Value < 1f) );
                case 799:
                    return ( (Value > 0f) && (Value < 1f) );
                case 155:
                    return ( (Value > -0.9f) && (Value < 1.3f) );
                case 196:
                    return ( (Value > -2f) && (Value < 1f) );
                case 193:
                    return ( (Value > 0f) && (Value < 1f) );
                case 608:
                    return ( (Value > 0f) && (Value < 1f) );
                case 609:
                    return ( (Value > 0f) && (Value < 1f) );
                case 646:
                    return ( (Value > -1.3f) && (Value < 1f) );
                case 647:
                    return ( (Value > -0.5f) && (Value < 1f) );
                case 649:
                    return ( (Value > 0f) && (Value < 1f) );
                case 682:
                    return ( (Value > 0f) && (Value < 1f) );
                case 690:
                    return ( (Value > 0f) && (Value < 1f) );
                case 795:
                    return ( (Value > 0f) && (Value < 1f) );
                case 841:
                    return ( (Value > -1f) && (Value < 1f) );
                case 753:
                    return ( (Value > -0.5f) && (Value < 3f) );
                case 676:
                    return ( (Value > -1f) && (Value < 2f) );
                case 863:
                    return ( (Value > 0f) && (Value < 1f) );
                case 16:
                    return ( (Value > -.5f) && (Value < 3f) );
                case 757:
                    return ( (Value > -4f) && (Value < 2f) );
                case 31:
                    return ( (Value > 0f) && (Value < 2f) );
                case 877:
                    return ( (Value > 0f) && (Value < 1f) );
            }
        }
    }
}
