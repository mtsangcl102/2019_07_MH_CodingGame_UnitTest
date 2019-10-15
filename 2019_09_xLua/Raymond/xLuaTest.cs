using UnityEngine;
using XLua;
using System;
using System.Collections.Generic;
using System.Text;

[CSharpCallLua]
public class xLuaTest : MonoBehaviour {
    LuaEnv _luaEnv = null;
    private List<int> testList = null;

    void OnGUI() {
        if (GUI.Button(new Rect(10, 10, 150, 100), "Reload Lua Text")) {
            RunTest();
        }
    }

    void Start() {
        _luaEnv = new LuaEnv();
    }

    void RunTest() {
        TestHelloWorld();
        TestImplementLogic();
        TestEncryptionScript();
        TestHotFix();
    }

    public void TestHelloWorld() {
        _luaEnv = new LuaEnv();
        _luaEnv.DoString( "require 'TestHelloWorld'");
        Debug.Log( "Case 1 : pass " );
    }
    
    [CSharpCallLua]
    public interface ICalendar {
        int Day_Of_Week(int dd, int mm, int yy);
        int Days_In_Month(int year, int month);
    }
    
    [CSharpCallLua]
    public delegate ICalendar CalendarNew();

    [CSharpCallLua]
    public void TestImplementLogic() {
        //try to do a simple logic of calendar
        //TODO: write a lua script name "TestImplement.lua.txt"
        _luaEnv.DoString("require 'TestImplement'");
        CalendarNew cal_new = _luaEnv.Global.GetInPath<CalendarNew>("Calendar.New");
        
        if ( (int)System.DateTime.DaysInMonth(2004, 2) - (int)cal_new().Days_In_Month( 2004, 2) != 0 )
            throw new Exception("Not same day of month");
        
        DateTime dt = new DateTime(2019, 9, 20);
        if( (int)dt.DayOfWeek - cal_new().Day_Of_Week( 2019, 9, 20) != 0 )
            throw new Exception("Not same day of week");
        
        Debug.Log( "Case 2 : pass " );
    }
    
    public delegate List<int> GetList();
    public void TestEncryptionScript()
    {
        LuaTable scriptEnv = _luaEnv.NewTable();
        EncryptionHelper _encryptionHelper = new EncryptionHelper();

        //TODO: use EncryptionHelper to load encrypted lua script
        ///use loader to decrypt script
        
        //Decrypt nonce = 102 //2019 - 09 - 30 update
        #region YourScript
        //TODO: loader here 
        
        _luaEnv.AddLoader( ( ref string filepath ) =>
        {
            var text = Resources.Load<TextAsset>( filepath ).text;
            var encryptedByteArray = Convert.FromBase64String( text );
            var decryptedByteArray = _encryptionHelper.Decrypt( encryptedByteArray , (long) 102 );
            // var decryptedString = Encoding.UTF8.GetString( decryptedByteArray ); 

            return decryptedByteArray;
        });
        
        _luaEnv.DoString("require 'Encrypted.lua'");
        _luaEnv.customLoaders.Clear();
        
        #endregion

        GetList unknowFunc = _luaEnv.Global.GetInPath<GetList>("_unknownFunc.unknownFunc");
        if ( 6 - unknowFunc().Count != 0 )
            throw new Exception("Length not valid");

        testList = new List<int>( unknowFunc() );
    }

    public void TestHotFix() {
        _luaEnv.DoString( "require 'TestLuaHotFix'");
        WrongFuncs wf = new WrongFuncs();
        
        int tempMulti = wf.PickMin(testList.ToArray());
        wf.TempActiveAttackUp(tempMulti);
        
        int dam_1 = 1000;
        int dam_2 = 10000;
        int dam_3 = 100000;
        
        CalDamageInstance.CalDamageEvent(ref dam_1);   
        CalDamageInstance.CalDamageEvent(ref dam_2);
        CalDamageInstance.CalDamageEvent(ref dam_3);
        
        //find min in C#
        int min = int.MaxValue;
        foreach (int num in testList) {
            min = num < min ? num : min;
        }
        
        //check case
        if ( 1000 * min != dam_1 )
            throw new Exception("Dam_1 cals wrong!!");
        
        if ( 10000 != dam_2 )
            throw new Exception("Dam_2 cals wrong!!");
        
        if ( 100000 != dam_3 )
            throw new Exception("Dam_3 cals wrong!!");

        //remove delegate
        wf.TempDeactiveAttackUp();
        
        int dam_4 = 100;
        CalDamageInstance.CalDamageEvent(ref dam_4);
        
        if ( 100 != dam_4 )
            throw new Exception("Dam_4 cals wrong!!");
        
        Debug.Log( "Case 3 : pass " );
    }
}

[Hotfix]
public class CalDamageInstance {
    public delegate void CalDamage(ref int oriDamage);

    public static event CalDamage calDamageEvent;

    public static void CalDamageEvent(ref int OriDamge) {
        if ( calDamageEvent != null )
            calDamageEvent(ref OriDamge);
    }
}

[Hotfix]
public class WrongFuncs {
    private int _multiper;

    public int PickMin( int[] arr ) {
        return 10000;
    }

    //Temp Attak up skill, if Damage < 10000 , Damage => Damage * _multiper
    public void TempActiveAttackUp( int multiper ) {
        //CalDamageInstance.calDamageEvent += CalDamageFunc;
    }

    public void CalDamageFunc(ref int OriDamge) {
        if (OriDamge == 10000)
            OriDamge += _multiper;
    }
    
    public void TempDeactiveAttackUp() {
        CalDamageInstance.calDamageEvent -= CalDamageFunc;
    }
}
















