# AeroGreen Core — API de Monitoramento e Automação de Estufas Adaptativas

O **AeroGreen Core** é o componente de backend (API RESTful) de um ecossistema distribuído focado no monitoramento e automação de estufas agrícolas adaptativas. Projetado para operar tanto em solo terrestre sob estresse climático severo quanto em módulos de simulação espacial (como colônias em Marte ou na Lua), o sistema realiza a ingestão e tratamento de telemetria ambiental em tempo real, disparando contingências automáticas e garantindo a resiliência do cultivo.

---

## 🌎 Alinhamento com os ODS da ONU
O projeto responde diretamente a desafios globais e de sustentabilidade:
* **Fome Zero e Agricultura Sustentável (ODS 2):** Protege culturas hidropônicas e aeropônicas contra falhas críticas de infraestrutura, reduzindo perdas e garantindo o suprimento de comida.
* **Ação Contra a Mudança Global do Clima (ODS 13):** Viabiliza a produção controlada de alimentos em zonas áridas, degradadas ou em atmosferas artificiais hostis.

---

## 🛠️ Arquitetura e Estrutura do Projeto
A API foi desenvolvida em **C# / .NET 8.0** utilizando **Dapper** para comunicação otimizada com o banco de dados **SQLite**, organizada estritamente em camadas:

```text
AgroControl-backend/
├── AgroControl-backend.sln       # Solução do Visual Studio
├── README.md                     # Este documento
├── agrocontrol/
│   ├── AgroControl.Api/          # API Principal
│   │   ├── Controllers/          # Exposição dos endpoints HTTP REST
│   │   ├── Services/             # Regras de negócio e avaliação de gatilhos
│   │   ├── Repositories/         # Acesso parametrizado ao SQLite via Dapper
│   │   ├── Models/               # Entidades e DTOs (Data Transfer Objects)
│   │   ├── Data/                 # Inicialização do DB SQLite
│   │   ├── Security/             # Sanitização contra XSS e validações
│   │   ├── sql/                  # Script DDL e seeds iniciais
│   │   └── appsettings.json      # Configurações do app
│   └── AgroControl.Tests/        # Projeto de Testes Unitários (xUnit)
│       ├── InputSanitizerTests.cs
│       └── TelemetriaServiceTests.cs
```

---

## 🏛️ Modelo Relacional do Banco de Dados (SQLite)
O banco de dados é estruturado seguindo rigorosas restrições relacionais (`NOT NULL`, `UNIQUE`, `CHECK` e `FOREIGN KEY`), composto por cinco tabelas centrais:

1. **`T_AGC_USUARIO`**: Operadores cadastrados no ecossistema.
2. **`T_AGC_ESTUFA`**: Módulos de estufa (identificados por localização e tipo de plantio).
3. **`T_AGC_DISPOSITIVO`**: Dispositivos IoT e sensores físicos associados a uma estufa.
4. **`T_AGC_TELEMETRIA_LOG`**: Histórico detalhado de leituras de sensores (Temperatura, Umidade, Água e Luminosidade).
5. **`T_AGC_ALERTA_EVENTO`**: Registro de alertas normais ou críticos disparados automaticamente.

---

## 🔒 Camada de Blindagem e Segurança
A API aplica conceitos modernos de proteção no tráfego e gravação de informações:
* **Criptografia de Senhas (Hash):** Uso da biblioteca **BCrypt.Net** para gerar hashes criptográficos unidirecionais das senhas de usuários antes da gravação no banco (`BCrypt.HashPassword`), bem como na validação do login (`BCrypt.Verify`).
* **Proteção contra SQL Injection (SQLi):** Todas as consultas no repositório utilizam parametrização nativa do Dapper. Concatenação de queries é estritamente proibida.
* **Sanitização de Entradas (Anti-XSS e Payload Corrompido):** O `InputSanitizer` filtra caracteres especiais e impede tags HTML (`<script>`, `onerror`, etc.) nos campos textuais.
* **Validação de Limites Físicos:** Telemetrias enviadas por dispositivos IoT que apresentam valores impossíveis na natureza (ex: temperatura acima de 80°C ou umidade negativa) são imediatamente rejeitadas antes de entrarem na camada de negócio.

---

## 📡 Endpoints da API

| Método | Endpoint | Descrição |
| :--- | :--- | :--- |
| **POST** | `/api/usuarios/create` | Cadastra novo operador com hash BCrypt na senha |
| **POST** | `/api/auth/login` | Valida credenciais criptografadas para acesso |
| **DELETE** | `/api/usuarios/delete/{id}` | Remove cadastro de um operador |
| **POST** | `/api/telemetria` | Processa e registra leituras de sensores IoT (gera alertas automáticos se houver anomalias) |
| **GET** | `/api/telemetria/recente/{id}` | Retorna o último log de telemetria de uma estufa |
| **GET** | `/api/alertas/criticos` | Lista todas as ocorrências de panes e eventos graves não resolvidos |
| **PUT** | `/api/alertas/{id}/resolver` | Resolve um alerta aberto (atualiza st_resolvido para 'S') |
| **DELETE** | `/api/telemetria/limpar-antigos` | Expurgos históricos de telemetria antigos via Query Parameter (`?dias=30`) |

---

## 🧪 Suíte de Testes Unitários
O projeto [AgroControl.Tests](file:///c:/Users/USUARIO/Downloads/AgroControl-backend/agrocontrol/AgroControl.Tests) cobre os cenários críticos com **39 casos de testes** estruturados em xUnit, testando:
1. **Regras do InputSanitizer:** Bloqueio de injeção XSS/HTML, validação do regex para UUIDs de estufas, e identificação de números não-finitos (`NaN`/`Infinity`).
2. **Regras do TelemetriaService:** Validação dos limites físicos e a lógica de geração de alertas automáticos baseada nos níveis dos sensores.

Para executar os testes, utilize o comando dentro do ambiente onde o .NET SDK está configurado:
```bash
dotnet test
```

---

## 🚀 Como Executar o Projeto Localmente
Como a aplicação está hospedada em um ambiente híbrido/WSL2, siga os passos abaixo para iniciar a API:

1. Acesse o diretório da API:
   ```bash
   cd agrocontrol/AgroControl.Api
   ```
2. Execute a aplicação utilizando o perfil HTTP:
   ```bash
   dotnet run --launch-profile http
   ```
3. A aplicação estará rodando em:
   * **[http://localhost:5062/swagger](http://localhost:5062/swagger)** (Swagger UI para testes rápidos dos endpoints).
   * **Atenção:** A rota raiz `/` não possui controller mapeado e retornará erro *404 Not Found*. Acesse sempre a rota `/swagger` no navegador para interagir com a interface visual da API.

---

## 👥 Participantes

Eduardo Gomes Pinho Junior - rm97919
Gustavo Ferreira Lopes - rm98887
Enzo de Oliveira Cunha - rm550985
Leonardo Viotti Bonini  - rm551716