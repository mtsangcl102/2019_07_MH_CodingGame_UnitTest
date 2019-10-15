using UnityEngine;
using XLua;
using System;
using System.Collections.Generic;
using System.Text;

[Hotfix]
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

	////start testing
	public class PropertyChangedEventArgs : EventArgs
	{
		public string name;
		public object value;
	}


	[CSharpCallLua]
	public interface ICalc
	{
		event EventHandler<PropertyChangedEventArgs> PropertyChanged;

		int Add( int a, int b );
		int Mult { get; set; }

		object this[ int index ] { get; set; }
	}

	[CSharpCallLua]
	public delegate ICalc CalcNew( int mult, params string[] args );


	//end testing


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

	/*
    [CSharpCallLua]
    public void TestImplementLogic() {
        //try to do a simple logic of calendar
        //TODO: write a lua script name "TestImplement.lua.txt"




        _luaEnv.DoString("require 'TestImplement'");


		//start test
        CalcNew calc_new = _luaEnv.Global.GetInPath<CalcNew>("Calc.New");
        ICalc calc = calc_new(10, "hi", "john"); //constructor
        Debug.Log("sum(*10) =" + calc.Add(1, 2));
		//end test

        CalendarNew cal_new = _luaEnv.Global.GetInPath<CalendarNew>("Calendar.New");
        
		//Debug.Log( (int)cal_new().Days_In_Month( 2004, 2) );
		//Debug.Log( "cprrect " + (int)System.DateTime.DaysInMonth(2004, 2) );
        if ( (int)System.DateTime.DaysInMonth(2004, 2) - (int)cal_new().Days_In_Month( 2004, 2) != 0 )
            throw new Exception("Not same day of month");
        
        DateTime dt = new DateTime(2019, 9, 20);

		Debug.Log( dt.DayOfWeek );
		Debug.Log( cal_new().Day_Of_Week( 2019, 9, 20) );
        if( (int)dt.DayOfWeek - cal_new().Day_Of_Week( 2019, 9, 20) != 0 )
            throw new Exception("Not same day of week");
        
        Debug.Log( "Case 2 : pass " );
    }
	*/
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
        
	    _luaEnv.AddLoader(
			(ref string filepath) =>
			{
				string text = ((TextAsset)Resources.Load( filepath +".lua" )).text;
				byte[] decrypted = _encryptionHelper.Decrypt( Convert.FromBase64String( text ), 102 );
				return decrypted;
			}
		);     
		_luaEnv.DoString( "require 'Encrypted'" );
        _luaEnv.customLoaders.Clear();

        #endregion

        GetList unknowFunc = _luaEnv.Global.GetInPath<GetList>("_unknownFunc.unknownFunc");
        if ( 6 - unknowFunc().Count != 0 )
            throw new Exception("Length not valid");

        testList = new List<int>( unknowFunc() );
    }

	/*
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
        
		string s = "rQBxZ/guypMA58hkVkQikL1wCFrroVCDWsmvZJADiH4rXXIBpJz0q2dvFt2ECLTAi/ZLG3Zw2aAJ8jOVSagCbDIKLyBDmf8yAScADTPDU5KensI/K/QSySjjE0SvqTURSmac/3guJMz7B4Ny1P1CkuK+4XONdiww+fkHcNaMy7fTQXA7KscD17p/LmX/LZDZ4vgMjXMncciNELTOiyoYmfHsoTZ6bYS2XneGzbreeoOdxTmM/JiIOV0QJ1Aq854PyCOa58TUEwo98wcBb4z7gXaBZjWnkJM2wjlJ9a26Wo3q51cbQRpdIyM+241Uu2PnMwVzDhITBHMf/74UBkoi8fViYAWhS89TKu7Uujc1pJ+x0vgTe/yLlElNQ4Npr4r4SDMNjbq5OCNN6pzE5pxILZyKjnQc8Y/aT1M9PJU8WXbgRD5nmshrkqe21hy/Y1wmB6UgpKyfqHfF0QOIeaAdIjwJtSBCTfIfEIEkuOOVaz/Io56I+d83TnSQ62kfr4LTqiDZ4B7YBb+XJUAAUEhswqbpna6xjTyHOjS+GSIaWltvtACWauHVqM5ExyAUoQeI+NTOWM/b9BRHO+20rCFd17XbS69EHwxQcCvMzroF26ll2Nzh/Lm3VM7YkH9f7Ubl/dyfgrmpZiKv769MKo6NRXuaoknHSnNzB6IXG+tyKsdPUIy6/M4G6NsxynDXgNGk1Uo9JulQfF5oTPi2BkQpeRQUDBRY4kYaZrJ7TnLU9v5PElcIkuqFgWdFFGoO/3U5gSqGIxeYxJiNg1d+Dt+/qwvSgrKoXQQYlnRWt01fgXZj";
		
        byte[] decrypted = _encryptionHelper.Decrypt( Convert.FromBase64String( s ), 102 );

		Debug.Log( Encoding.UTF8.GetString( decrypted ) );
       // scriptEnv.DoString("require 'TestImplement'");
        
        
        _luaEnv.DoString( Encoding.UTF8.GetString( decrypted ) );
        
        
        
        #endregion

        GetList unknowFunc = _luaEnv.Global.GetInPath<GetList>("_unknownFunc.unknownFunc");
        if ( 6 - unknowFunc().Count != 0 )
            throw new Exception("Length not valid");

        testList = new List<int>( unknowFunc() );
    }
	*/





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


/*
    public void TestHotFix() {
		 _luaEnv.DoString( "require 'TestLuaHotFix'");

		//_luaEnv.DoString( @"
		//              xlua.hotfix(CS.WrongFuncs, 'CalDamageFunc', function(self, a)													
		//					if a < 10000 then return a * self._multiper end
		//					return a
		//              end)

		//            xlua.hotfix(CS.WrongFuncs, 'PickMin', function(self, a)
		//				local b = 9999
		//				for i = 0, a.Length - 1 do
		//					if a[ i ] < b then b = a[ i ] end
		//				end
		//                return b	                  
		//            end)


		//			local CalDamageFunc1

		//              xlua.hotfix(CS.WrongFuncs, 'TempActiveAttackUp', function(self, a)
		//					self._multiper = a

		//					CalDamageFunc1 = function( aa )
		//						return self:CalDamageFunc( aa )
		//					end

		//					CS.CalDamageInstance.calDamageEvent('+', CalDamageFunc1 )
		//              end)

		//			  xlua.hotfix(CS.WrongFuncs, 'TempDeactiveAttackUp', function(self)
		//				CS.CalDamageInstance.calDamageEvent('-', CalDamageFunc1 )
		//			  end)
		//	" );
			

			//if a < 10000 then return a end	
			//if a >= 10000 then return a * self._multiper end
			//CS.CalDamageInstance:AAAA( '+', self.AAA )
			//							CS.CalDamageInstance:calDamageEvent('+', CalDamageFunc)

		//_luaEnv.DoString( @"
		//    xlua.hotfix(CS.xLuaTest, 'Update', function(self)
		//        self.tick = self.tick + 1
		//        if (self.tick % 50) == 0 then
		//            print('<<<<<<<<Update in lua, tick = ' .. self.tick)
		//        end
		//    end)
		//" );

		WrongFuncs wf = new WrongFuncs();
		wf.AAA();
		wf.Update();
		
		Debug.Log("pickMin " + wf.PickMin(testList.ToArray() ) );
        int tempMulti = wf.PickMin(testList.ToArray());
        wf.TempActiveAttackUp(tempMulti);

		//CalDamageInstance.AAAAA();

        int dam_1 = 1000;
        int dam_2 = 10000;
        int dam_3 = 100000;
        
        CalDamageInstance.CalDamageEvent(ref dam_1);   
        CalDamageInstance.CalDamageEvent(ref dam_2);
        CalDamageInstance.CalDamageEvent(ref dam_3);
        
        //find min in C#
        int min = int.MaxValue;
		string testing = "";
        foreach (int num in testList) {
            min = num < min ? num : min;
			testing += num +" ";
        }
        Debug.Log("dam " + dam_1 + " " + dam_2 +" "+ dam_3 );
		Debug.Log("test " + testing );
		Debug.Log("min " + min );
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
	
	public delegate void BBBB();
	public static event BBBB AAAA;

    public static void CalDamageEvent(ref int OriDamge) {
        if ( calDamageEvent != null )
            calDamageEvent(ref OriDamge);
    }

	public static void AAAAA()
	{
		AAAA();
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

	//testing
	public void AAA()
	{
		Debug.Log("aaa");
	}
	int tick = 49;
    public void Update()
    {
        if (++tick % 50 == 0)
        {
            Debug.Log(">>>>>>>>Update in C#, tick = " + tick);
        }
    }
	//end testing

    public void CalDamageFunc(ref int OriDamge) {
        if (OriDamge == 10000)
            OriDamge += _multiper;
    }
    
    public void TempDeactiveAttackUp() {
        CalDamageInstance.calDamageEvent -= CalDamageFunc;
    }
}
*/















