using System.Linq;
using KMHelper;
using System;
using Random = UnityEngine.Random;
using UnityEngine;

public class timekeeper : MonoBehaviour {

    private static int _moduleIdCounter = 1;
    public KMAudio newAudio;
    public KMBombModule module;
    public KMBombInfo info;
    private int _moduleId = 0;
    private bool _isSolved = false, _lightsOn = false;

    public KMSelectable LEDOne;
    public KMSelectable LEDTwo;
    public KMSelectable LEDThree;
    private KMSelectable[] LEDs;
    public TextMesh displayedNumberText;
    private int displayedNumber;

    public MeshRenderer LEDOneMesh;
    public MeshRenderer LEDTwoMesh;
    public MeshRenderer LEDThreeMesh;

    private int LEDOneColor;
    private int LEDTwoColor;
    private int LEDThreeColor;
    private int displayedTextColor;

    private int red = 0;
    private int blue = 1;
    private int yellow = 2;
    private int green = 3;
    private int white = 4;
    private int black = 5;

    public Material[] colors = new Material[6]; //0=red,1=blue,2=yellow,3=green,4=white,5=black
    public String[] colorNames = new String[6]; //To make debug messages easier
    private Color[] textColors = new Color[5];  //0=red,1=blue,2=yellow,3=green,4=white

    private int month;

    private int correctLEDIndex;
    private int correctTime = 0;

    private bool end = false;

    char[] letters;
    int[] letterIndexes;
    int[] numbers;
    int batteryCount;
    int batteryHolderCount;
    int portPlateCount;
    int portCount;
    int litCount;
    int unlitCount;

    void Start()
    {
        _moduleId = _moduleIdCounter++;
        module.OnActivate += Activate;
    }

    private void Awake()
    {
        LEDOne.OnInteract += delegate
        {
            handleLEDPress(0);
            return false;
        };
        LEDTwo.OnInteract += delegate
        {
            handleLEDPress(1);
            return false;
        };
        LEDThree.OnInteract += delegate
        {
            handleLEDPress(2);
            return false;
        };
    }

    void handleLEDPress(int index)
    {
        int timeLeft = (int)info.GetTime();
        while (correctTime - timeLeft > 2)
        {
            correctTime = (int)Math.Floor((double)(correctTime / 2));
        } 
        newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.WireSnip, LEDs[index].transform);
        if (_isSolved || !_lightsOn) return;
        if (index == correctLEDIndex)
        {
            if (Math.Abs(timeLeft - correctTime) <= 2)
            {
                if (timeLeft > 10)
                {
                    newAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, LEDs[index].transform);
                } else
                {
                    module.HandleStrike();
                }
                module.HandlePass();
            } else
            {
                Debug.LogFormat("[TimeKeeper #{0}] Correct LED pressed at wrong time. Expected time: {1}. Recieved time: {2}.", _moduleId, correctTime, timeLeft);
                module.HandleStrike();
            }
        } else
        {
            Debug.LogFormat("[TimeKeeper #{0}] Incorrect LED pressed. Expected {1}. Recieved: {2}.", _moduleId, correctLEDIndex+1, index+1);
            module.HandleStrike();
        }
    }

    void Activate()
    {
        Init();
        _lightsOn = true;
    }

    void Init()
    {
        correctLEDIndex = -1;
        LEDs = new KMSelectable[3] { LEDOne, LEDTwo, LEDThree };
        letters = info.GetSerialNumberLetters().ToArray();
        numbers = info.GetSerialNumberNumbers().ToArray();
        batteryCount = info.GetBatteryCount();
        batteryHolderCount = info.GetBatteryHolderCount();
        portPlateCount = info.GetPortPlateCount();
        portCount = info.GetPortCount();
        litCount = info.GetOnIndicators().Count();
        unlitCount = info.GetOffIndicators().Count();
        letterIndexes = new int[letters.Length];
        for(int i = 0; i<letterIndexes.Length; i++)
        {
            letterIndexes[i] = GetIndexInAlphabet(letters[i]);
        }
        displayedNumber = Random.Range(0, 51);
        string text = displayedNumber.ToString();
        if(displayedNumber < 10)
        {
            displayedNumberText.text = "0" + text;
        } else
        {
            displayedNumberText.text = text;
        }
        int rndColor = Random.Range(0, 5);
        setupTextColors();
        displayedNumberText.color = textColors[rndColor];
        displayedTextColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDOneMesh.material = colors[rndColor];
        LEDOneColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDTwoMesh.material = colors[rndColor];
        LEDTwoColor = rndColor;
        rndColor = Random.Range(0, 6);
        LEDThreeMesh.material = colors[rndColor];
        LEDThreeColor = rndColor;
        month = DateTime.Today.Month;
        Debug.LogFormat("[TimeKeeper #{0}] Displayed Number: {5}. LED One color: {1}. LED Two color: {2}. LED Three color: {3}. Displayed number color: {4}.", _moduleId, colorNames[LEDOneColor], colorNames[LEDTwoColor], colorNames[LEDThreeColor], colorNames[displayedTextColor], displayedNumber);
        getCorrectAnswer();
    }

    void setupTextColors()
    {
        textColors[0] = new Color(255, 0, 0);
        textColors[1] = new Color(0, 0, 255);
        textColors[2] = new Color(255, 255, 0);
        textColors[3] = new Color(0, 255, 0);
        textColors[4] = new Color(255, 255, 255);
    }

    void getCorrectAnswer()
    {
        for(int i=1; i<22; i++)
        {
            if (end) break;
            steps(i);
        }
        if(correctLEDIndex == -1)
        {
            getCorrectLED();
        }
        Debug.LogFormat("[TimeKeeper #{0}] Correct LED: {1}. Correct time (in seconds): {2}.", _moduleId, correctLEDIndex + 1, correctTime);
    }

    private bool rule9 = false;

    void steps(int step)
    {
        switch (step)
        {
            case 1:
                correctTime = displayedNumber;
                for(int i = 0; i < letterIndexes.Length; i++)
                {
                    correctTime += letterIndexes[i];
                    Debug.Log("Rule 1 adding the letter " + letters[i] + ", which has a number value of " + letterIndexes[i]);
                }
                for(int n =0; n < numbers.Length; n++)
                {
                    correctTime -= numbers[n];
                }
                Debug.Log("Rule 1 used.");
                
                break;
            case 2:
                if(LEDOneColor == white)
                {
                    correctTime += 14;
                    Debug.Log("Rule 2 used.");
                }
                break;
            case 3:
                if(LEDTwoColor == displayedTextColor)
                {
                    correctTime += 22;
                    Debug.Log("Rule 3 = true.");
                } else
                {
                    correctTime += 13;
                    Debug.Log("Rule 3 = false.");
                }
                break;
            case 4:
                correctTime += (2 * portPlateCount);
                Debug.Log("Rule 4 added " + (2 * portPlateCount));
                if (info.IsPortPresent(KMBombInfoExtensions.KnownPortType.DVI))
                {
                    correctTime -= 9;
                    Debug.Log("Rule 4: DVI found.");
                }
                break;
            case 5:
                if(LEDOneColor == LEDTwoColor && LEDOneColor == LEDThreeColor)
                {
                    setCorrectLED(0);
                    Debug.Log("Rule 5 used.");
                }
                break;
            case 6:
                if((displayedTextColor == red || displayedTextColor == green || displayedTextColor == blue) && (LEDOneColor != yellow && LEDTwoColor != yellow && LEDThreeColor != yellow))
                {
                    correctTime += displayedNumber;
                    Debug.Log("Rule 6 used.");
                }
                break;
            case 7:
                if (info.GetSolvableModuleNames().Count() > (batteryCount+batteryHolderCount))
                {
                    correctTime -= 18;
                    Debug.Log("Rule 7 used.");
                }
                break;
            case 8:
                if(correctTime % 2 == 0 && correctTime > 72)
                {
                    correctTime /= 2;
                    Debug.Log("Rule 8 used.");
                }
                break;
            case 9:
                if(LEDTwoColor == green || LEDTwoColor == black)
                {
                    setCorrectLED(1);
                    rule9 = true;
                    Debug.Log("Rule 9 used.");
                }
                break;
            case 10:
                if((correctTime % 23) < (2 * portCount))
                {
                    end = true;
                    Debug.Log("Rule 10 used.");
                }
                break;
            case 11:
                correctTime += month;
                Debug.Log("Rule 11 used.");
                break;
            case 12:
                if (displayedNumber > 23)
                {
                    correctTime += batteryHolderCount;
                    Debug.Log("Rule 12 = true.");
                } else
                {
                    correctTime *= batteryHolderCount;
                    Debug.Log("Rule 12 = false.");
                }
                break;
            case 13:
                correctTime += (2 * litCount);
                correctTime -= (3 * unlitCount);
                Debug.Log("Rule 13 added " + (2 * litCount) + " and subtracted " + (3 * unlitCount));
                break;
            case 14:
                if (LEDOneColor == displayedTextColor && LEDOneColor == LEDThreeColor && LEDOneColor != LEDTwoColor)
                {
                    setCorrectLED(2);
                    end = true;
                    Debug.Log("Rule 14 used.");
                }
                break;
            case 15:
                if (rule9)
                {
                    correctTime += 10;
                    Debug.Log("Rule 15 = true.");
                } else
                {
                    correctTime -= 19;
                    Debug.Log("Rule 15 = false.");
                }
                break;
            case 16:
                if(correctTime < 0)
                {
                    correctTime *= -2;
                    end = true;
                    Debug.Log("Rule 16 used.");
                }
                break;
            case 17:
                correctTime *= 3;
                Debug.Log("Rule 17 used.");
                break;
            case 18:
                if((colorNames[LEDOneColor].Length + colorNames[LEDTwoColor].Length + colorNames[LEDThreeColor].Length) > 13)
                {
                    correctTime += colorNames[displayedTextColor].Length;
                    Debug.Log("Rule 18 used.");
                }
                break;
            case 19:
                if(portPlateCount == 0)
                {
                    end = true;
                    Debug.Log("Rule 19 used.");
                }
                break;
            case 20:
                if (info.IsIndicatorPresent(KMBombInfoExtensions.KnownIndicatorLabel.FRK)){
                    Debug.Log("Rule 20 FRK out used.");
                    break;
                }
                if(colorNames[LEDOneColor].Length > colorNames[LEDTwoColor].Length && colorNames[LEDOneColor].Length > colorNames[LEDThreeColor].Length)
                {
                    setCorrectLED(0);
                    Debug.Log("Rule 20 LED 1 used.");
                } else if (colorNames[LEDTwoColor].Length > colorNames[LEDOneColor].Length && colorNames[LEDTwoColor].Length > colorNames[LEDThreeColor].Length)
                {
                    setCorrectLED(1);
                    Debug.Log("Rule 20 LED 2 used.");
                }
                else if (colorNames[LEDThreeColor].Length > colorNames[LEDTwoColor].Length && colorNames[LEDThreeColor].Length > colorNames[LEDOneColor].Length)
                {
                    setCorrectLED(2);
                    Debug.Log("Rule 20 LED 3 used.");
                } else
                {
                    Debug.Log("Rule 20 Tie");
                }
                break;
            case 21:
                if(unlitCount == 0)
                {
                    correctTime *= 3;
                    Debug.Log("Rule 21 no unlit.");
                    break;
                } 
                foreach(string label in info.GetOffIndicators())
                {
                    correctTime += (GetIndexInAlphabet(label[0]));
                    Debug.Log("Rule 21: added " + label);
                }
                break;
        }
    }

    void getCorrectLED()
    {
        if(correctTime < 100)
        {
            correctLEDIndex = 0;
        } else if (displayedTextColor == green && LEDOneColor != green)
        {
            correctLEDIndex = 2;
        } else if(LEDOneColor != LEDTwoColor && LEDOneColor != LEDThreeColor && LEDOneColor != displayedTextColor && LEDTwoColor != LEDThreeColor && LEDTwoColor != displayedTextColor && LEDThreeColor != displayedTextColor)
        {
            correctLEDIndex = 0;
        } else if (info.IsPortPresent(KMBombInfoExtensions.KnownPortType.Parallel))
        {
            correctLEDIndex = 1;
        } else
        {
            correctLEDIndex = 2;
        }
    }

    private static int GetIndexInAlphabet(char value)
    {
        switch (value)
        {
            case 'A':
                return 1;
            case 'B':
                return 2;
            case 'C':
                return 3;
            case 'D':
                return 4;
            case 'E':
                return 5;
            case 'F':
                return 6;
            case 'G':
                return 7;
            case 'H':
                return 8;
            case 'I':
                return 9;
            case 'J':
                return 10;
            case 'K':
                return 11;
            case 'L':
                return 12;
            case 'M':
                return 13;
            case 'N':
                return 14;
            case 'O':
                return 15;
            case 'P':
                return 16;
            case 'Q':
                return 17;
            case 'R':
                return 18;
            case 'S':
                return 19;
            case 'T':
                return 20;
            case 'U':
                return 21;
            case 'V':
                return 22;
            case 'W':
                return 23;
            case 'X':
                return 24;
            case 'Y':
                return 25;
            case 'Z':
                return 26;
            default:
                return 0;
        }
    }

    void setCorrectLED(int index)
    {
        if (correctLEDIndex == -1)
        {
            correctLEDIndex = index;
        }
    }
}
