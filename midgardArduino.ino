#include <Adafruit_NeoPixel.h>
#include <SoftwareSerial.h>

#define PIN_r 7
#define SENSOR_HALL_PIN 4
#define RINGPIXELS 12 
#define BT_RX 10 
#define BT_TX 11 

Adafruit_NeoPixel ring = Adafruit_NeoPixel(RINGPIXELS, PIN_r, NEO_GRB + NEO_KHZ800);
SoftwareSerial bluetooth(BT_RX, BT_TX);

int ultimoEstadoSensor = HIGH; 
int ultimoGrupoSorteado = -1; 
int repeticoesSeguidas = 0;
const int BRILHO_PADRAO = 150;

void setup() {
  pinMode(SENSOR_HALL_PIN, INPUT_PULLUP);
  Serial.begin(9600);
  bluetooth.begin(9600);
  
  ring.begin();
  ring.setBrightness(BRILHO_PADRAO); 
  ring.clear();
  ring.show(); 
  
  randomSeed(analogRead(0)); 
  ultimoEstadoSensor = digitalRead(SENSOR_HALL_PIN);
}

void loop() {
  if (bluetooth.available() > 0) {
    String comando = bluetooth.readStringUntil('\n');
    comando.trim();
    comando.toUpperCase();
    processarComando(comando);
  }

  int estadoAtualSensor = digitalRead(SENSOR_HALL_PIN);
  if (estadoAtualSensor == LOW && ultimoEstadoSensor == HIGH) {
    delay(50); 
    girarRoleta(sortearComProbabilidade());
  }
  ultimoEstadoSensor = estadoAtualSensor; 
}

// --- Lógica de Sorteio e Comandos ---

int sortearComProbabilidade() {
  int grupoSorteado;
  bool sorteioValido = false;

  while (!sorteioValido) {
    int chance = random(0, 100);
    if (chance < 40) grupoSorteado = 0;      // Azul
    else if (chance < 80) grupoSorteado = 2; // Roxo
    else if (chance < 91) grupoSorteado = 3; // Amarelo
    else grupoSorteado = 1;                  // Vermelho

    if (grupoSorteado == ultimoGrupoSorteado) {
      if (repeticoesSeguidas < 2) {
        repeticoesSeguidas++;
        sorteioValido = true;
      }
    } else {
      ultimoGrupoSorteado = grupoSorteado;
      repeticoesSeguidas = 1;
      sorteioValido = true;
    }
  }
  return (grupoSorteado * 3) + random(0, 3);
}

void processarComando(String cmd) {
  int alvo = -1;
  
  if (cmd.indexOf("NEVASCA") >= 0) {
    efeitoSimples(ring.Color(255, 255, 255)); 
    return;
  } 
  if (cmd.indexOf("MARE") >= 0) {
    efeitoSimples(ring.Color(0, 0, 255)); 
    return;
  } 
  if (cmd.indexOf("HUNT") >= 0) {
    efeitoSimples(ring.Color(255, 0, 0)); 
    return;
  }

  if (cmd.indexOf("AZUL") >= 0)           alvo = random(0, 3); 
  else if (cmd.indexOf("VERMELHA") >= 0)  alvo = random(3, 6); 
  else if (cmd.indexOf("ROXA") >= 0)      alvo = random(6, 9); 
  else if (cmd.indexOf("AMARELA") >= 0)   alvo = random(9, 12); 
  else if (cmd.indexOf("GIRAR") >= 0)     alvo = sortearComProbabilidade();

  if (alvo != -1) girarRoleta(alvo);
}

// --- Efeitos e Animações ---

void efeitoSimples(uint32_t cor) {
  preencherAnel(cor);
  delay(2000); // Fica aceso 2s
  executarFadeout(cor);
}

void executarFadeout(uint32_t cor) {
  // Fadeout de aproximadamente 1 segundo
  for (int b = BRILHO_PADRAO; b >= 0; b -= 5) {
    ring.setBrightness(b);
    preencherAnel(cor); 
    delay(30); 
  }
  ring.clear();
  ring.show();
  ring.setBrightness(BRILHO_PADRAO); // Reseta brilho para o próximo uso
}

uint32_t getZoneColor(int ledIndex) {
  if (ledIndex >= 0 && ledIndex <= 2) return ring.Color(0, 0, 255);   // Azul
  if (ledIndex >= 3 && ledIndex <= 5) return ring.Color(255, 0, 0);   // Vermelho
  if (ledIndex >= 6 && ledIndex <= 8) return ring.Color(160, 0, 255); // Roxo Ajustado
  return ring.Color(255, 200, 0);                                    // Amarelo
}

void preencherAnel(uint32_t cor) {
  for(int i=0; i<RINGPIXELS; i++) ring.setPixelColor(i, cor);
  ring.show();
}

uint32_t diminuirBrilho(uint32_t cor, float fator) {
  uint8_t r = (uint8_t)((cor >> 16 & 0xFF) * fator);
  uint8_t g = (uint8_t)((cor >> 8 & 0xFF) * fator);
  uint8_t b = (uint8_t)((cor & 0xFF) * fator);
  return ring.Color(r, g, b);
}

void girarRoleta(int paradaFinal) {
  int voltasExtras = 4; 
  int totalPassos = (voltasExtras * RINGPIXELS) + paradaFinal;
  int wait = 30; 

  // Animação de giro
  for (int i = 0; i <= totalPassos; i++) {
    int ledAtual = i % RINGPIXELS;
    int ledTras1 = (ledAtual - 1 + RINGPIXELS) % RINGPIXELS;
    int ledTras2 = (ledAtual - 2 + RINGPIXELS) % RINGPIXELS;

    ring.clear();
    uint32_t corBase = getZoneColor(ledAtual);
    ring.setPixelColor(ledAtual, corBase);
    ring.setPixelColor(ledTras1, diminuirBrilho(corBase, 0.3));
    ring.setPixelColor(ledTras2, diminuirBrilho(corBase, 0.1));
    ring.show();

    if (i > totalPassos - 10) wait += 45;
    delay(wait);
  }

  uint32_t corVencedora = getZoneColor(paradaFinal);
  
  // Pisca Vencedor
  for (int j = 0; j < 6; j++) {
    if (j % 2 == 0) preencherAnel(corVencedora);
    else preencherAnel(0);
    delay(250);
  }

  // Fica aceso antes do fadeout
  preencherAnel(corVencedora);
  delay(2000); 

  // Novo Fadeout após a roleta
  executarFadeout(corVencedora);
}