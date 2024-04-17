#include <Arduino.h>
#include <WiFi.h>
#include <WiFiMulti.h>
#include <WiFiClientSecure.h>
#include <WebSocketsClient.h>
#include <ArduinoJson.h>
#include <ESP32Encoder.h>

// --- Для энкодера ---
ESP32Encoder encoder1;
ESP32Encoder encoder2;

#define EncPR1 12
#define EncPL1 13
#define EncPR2 32
#define EncPL2 33
// --------------------

// --- Для мотора ---
#define MtrS1 25
#define MtrR1 26
#define MtrL1 27

#define MtrS2 21
#define MtrR2 18
#define MtrL2 19

WebSocketsClient webSocket;
WiFiClient client;

// --- Данные о сети WIFI ---
const char* ssid = "Elena";
const char* password = "21011970";
// --------------------------

// --- Данные подключения к серверу ---
char* ADDR = "192.168.0.9";
const uint16_t PORT = 8080;
char* URL = "/?user=arduino";
// ------------------------------------

char line[64] = "";

// --- Энкодер ---
int pos = 0;
int totalPos = 0;

int enc1Position = 0;
int enc2Position = 0;
int lastEnc1Position = 0;
int lastEnc2Position = 0;
int serverEnc1Position = 0;
int serverEnc2Position = 0;

int lastTotalPos = 0;
byte lastState = 0;
const int8_t increment[16] = {0, -1, 1, 0, 1, 0, 0, -1, -1, 0, 0, 1, 0, 1, -1, 0};
bool serverUpdate = false;
// ---------------

// --- Двигатель ---
int posFromServer = 0;
struct MotorState {
  bool engineWorking;
  bool rotateRight;
  bool rotateLeft;
  bool motorStart;
};

DynamicJsonDocument doc(1024);



void webSocketEvent(WStype_t type, uint8_t * payload, size_t length) 
{
  switch(type) {
		case WStype_DISCONNECTED:
		  Serial.printf("[WSc] Disconnected!\n");
			break;
		case WStype_CONNECTED:
			Serial.printf("[WSc] Connected to url: %s\n", payload);
			break;
    case WStype_TEXT:
      for (int i = 0; i < length; i++)
      {
        line[i] += (char)payload[i];
      }
      serverUpdate = true;
      //Serial.printf("[WSc] get text: %s\n", action, id, value);
      break;  
    case WStype_BIN:
      Serial.printf("[WSc] get binary length: %u\n", length);
      break;
  }
}



void ServerConnecting()
{
  
  WiFi.begin(ssid, password);
  Serial.println();
  Serial.print("Connecting to the WIFI network");

  while(WiFi.status() != WL_CONNECTED)
  {
    Serial.print(".");
    delay(100);
  }

  Serial.println("\nWIFI Connected!");
  Serial.print("Local ESP32 IP: ");
  Serial.println(WiFi.localIP());
  delay(1000);

  Serial.print("\nConnected to the server");
  webSocket.begin(ADDR, PORT, URL);
  webSocket.onEvent(webSocketEvent);
  webSocket.setReconnectInterval(5000);
}



// --- Отправка значений на сервер ---
void sendingServerValues(int id, int encPosition, int& lastEncPosition, MotorState& state)
{
  if ((lastEncPosition != encPosition) && !serverUpdate && (state.engineWorking == false))
  {
    Serial.println("Send: " + String((int32_t)encPosition));
    Serial.println("lastEncPosition: " + String((int32_t)lastEncPosition) + "; encPosition: " + String((int32_t)encPosition) + "; serverUpdate: " + serverUpdate + "; engineWork: " + state.engineWorking);
    Serial.println("");
    doc["action"] = "SENDVALVE";
    doc["id"] = id;
    doc["value"] = encPosition;

    String jsonString;
    serializeJson(doc, jsonString);
    webSocket.sendTXT(jsonString);

    lastEncPosition = encPosition;
  }
}



// --- Применение значения полученного с сервера ---
void applyingServerValues(int MtrR, int MtrL, int MtrS, int positionFromServer, int& encPosition, MotorState& state)
{
  if ((positionFromServer > encPosition) && state.motorStart && (state.rotateRight == false))
  {
    state.engineWorking = true;
    state.rotateRight = true;
    analogWrite( MtrS, 102 );
    digitalWrite( MtrR, LOW );
    digitalWrite( MtrL, HIGH );
    Serial.printf("positionFromServer: ");
    Serial.print(positionFromServer);
    Serial.print("; encPosition: ");
    Serial.print(encPosition);
    Serial.println("");
  }
  if ((positionFromServer < encPosition) && state.motorStart && (state.rotateLeft == false))
  {
    state.engineWorking = true;
    state.rotateLeft = true;
    analogWrite( MtrS, 102 );
    digitalWrite( MtrR, HIGH );
    digitalWrite( MtrL, LOW );
    Serial.printf("positionFromServer: ");
    Serial.print(positionFromServer);
    Serial.print(" encPosition: ");
    Serial.print(encPosition);
    Serial.println("");
  }
  if ((positionFromServer == encPosition) && state.engineWorking == true)
  {
    Serial.println("Worked for positionFromServer: " + positionFromServer);
    state.engineWorking = false;
    state.rotateRight = false;
    state.rotateLeft = false;
    state.motorStart = false;
    digitalWrite( MtrR, LOW );
    digitalWrite( MtrL, LOW );
    digitalWrite( MtrS, 0 );
  }
}



void setup() 
{
  Serial.begin(115200);

  ESP32Encoder::useInternalWeakPullResistors = puType::up;
  encoder1.attachHalfQuad(EncPR1, EncPL1);
  encoder2.attachHalfQuad(EncPR2, EncPL2);
  encoder1.clearCount();
  encoder2.clearCount();

  pinMode( MtrS1, OUTPUT );
  pinMode( MtrR1, OUTPUT );
  pinMode( MtrL1, OUTPUT );

  pinMode( MtrS2, OUTPUT );
  pinMode( MtrR2, OUTPUT );
  pinMode( MtrL2, OUTPUT );

  ServerConnecting();
}



void loop()
{
  webSocket.loop();

  static MotorState state1 = {false, false, false, false};
  static MotorState state2 = {false, false, false, false};

  totalPos = encoder1.getCount() / 2;
  enc1Position = encoder1.getCount() / 2;
  enc2Position = encoder2.getCount() / 2;

  // --- Получение значения с сервера ---
  if (*line)
  {
    deserializeJson(doc, line);
    String action = doc["action"];
    String id = doc["id"];
    String value = doc["value"];
    memset(line, 0, 64);
    Serial.print("[WSc] get text: ");
    Serial.print("action: ");
    Serial.print(action);
    Serial.print("; id: ");
    Serial.print(id);
    Serial.print("; value: ");
    Serial.print(value);
    Serial.println("");

    if (id.toInt() == 1)
    {
      serverEnc1Position = value.toInt();
      state1.motorStart = true;
      Serial.println("Value for ID 1: " + String((int32_t)serverEnc1Position) + "; Encoder position: " + String((int32_t)enc1Position));
    }

    if (id.toInt() == 2)
    {
      serverEnc2Position = value.toInt();
      state2.motorStart = true;
      Serial.println("Value for ID 2: " + String((int32_t)serverEnc2Position) + "; Encoder position: " + String((int32_t)enc2Position));
    }

    if (id.toInt() == 3)
    {
      Serial.print("Stats: enc1Position: ");
      Serial.print(enc1Position);
      Serial.print("; enc2Position: ");
      Serial.println(enc2Position);

      Serial.print("Stats: serverEnc1Position: ");
      Serial.print(serverEnc1Position);
      Serial.print("; serverEnc2Position: ");
      Serial.println(serverEnc2Position);

      Serial.print("Stats: lastEnc1Position: ");
      Serial.print(lastEnc1Position);
      Serial.print("; lastEnc2Position: ");
      Serial.println(lastEnc2Position);

      Serial.print("Stats: state1.engineWorking: ");
      Serial.print(state1.engineWorking);
      Serial.print("; state1.rotateRight: ");
      Serial.print(state1.rotateRight);
      Serial.print("; state1.rotateLeft: ");
      Serial.println(state1.rotateLeft);

      Serial.print("Stats: state2.engineWorking: ");
      Serial.print(state2.engineWorking);
      Serial.print("; state2.rotateRight: ");
      Serial.print(state2.rotateRight);
      Serial.print("; state2.rotateLeft: ");
      Serial.println(state2.rotateLeft);
      Serial.println("");
      Serial.println("");
      Serial.println("");
    }
  }
  // ----------------------------------



  applyingServerValues(MtrR1, MtrL1, MtrS1, serverEnc1Position, enc1Position, state1);
  applyingServerValues(MtrR2, MtrL2, MtrS2, serverEnc2Position, enc2Position, state2);

  sendingServerValues(1, enc1Position, lastEnc1Position, state1);
  sendingServerValues(2, enc2Position, lastEnc2Position, state2);

  serverUpdate = false;
}
