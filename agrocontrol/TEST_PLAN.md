# Plano de Testes — AgroControl API (via Swagger UI)

App rodando localmente: `http://localhost:5062/swagger`
DB: SQLite (seed: Estufa id=1 "Estufa Marte" + Dispositivo `ESTUFA-MARTE-01`).

Cada teste abaixo produz um resultado visivelmente diferente se a implementação estiver quebrada.

## Cenário 1 — Cadastro com sucesso (POST /api/usuarios/create)
- Body: `{"nome":"Gustavo Operador","email":"gustavo@agro.com","senha":"Senha@123"}`
- **PASS**: HTTP 201, body `{"sucesso":true,"mensagem":"Usuario criado com sucesso. Id=1"}`.
- Quebrado se: 500, ou senha salva em texto puro (provado no Cenário 2 — login só funciona se o hash BCrypt foi gravado e verificado).

## Cenário 2 — Login com sucesso (POST /api/auth/login)
- Body: `{"email":"gustavo@agro.com","senha":"Senha@123"}`
- **PASS**: HTTP 200, `{"sucesso":true,"mensagem":"Login efetuado com sucesso. Bem-vindo(a), Gustavo Operador."}`.
- Adversarial: repetir com senha `"errada"` → **PASS** se HTTP 401 `{"sucesso":false,"mensagem":"Credenciais invalidas."}`. (Prova que BCrypt.Verify discrimina.)

## Cenário 3 — Telemetria Normal (POST /api/telemetria)
- Body: `{"codigoUuid":"ESTUFA-MARTE-01","temperatura":24.5,"umidade":60,"agua":80,"luminosidade":40000}`
- **PASS**: HTTP 201, `alertasGerados` é **lista vazia `[]`** (valores dentro da faixa ideal → nenhum alerta).
- Quebrado se: gera alertas indevidos, ou erro.

## Cenário 4 — Telemetria Crítica gera alerta automático (POST /api/telemetria)
- Body: `{"codigoUuid":"ESTUFA-MARTE-01","temperatura":52,"umidade":12,"agua":5,"luminosidade":150000}`
- **PASS**: HTTP 201, `alertasGerados` contém **3 itens "Critico"** (TEMPERATURA superaquecimento, UMIDADE ressecamento, AGUA reservatório) + 1 "Alerta" (luminosidade).
- Confirmação: GET /api/alertas/criticos retorna os 3 críticos com `stResolvido:"N"`.

## Cenário 5 — Ingestão de dado inválido bloqueada (POST /api/telemetria)
- Body com payload corrompido/XSS: `{"codigoUuid":"<script>alert(1)</script>","temperatura":24,"umidade":60,"agua":80,"luminosidade":40000}`
- **PASS**: HTTP 400, `{"sucesso":false,"mensagem":"CodigoUuid invalido: use apenas letras, numeros, hifen e underscore."}`.
- Variante (limite físico): `temperatura:999` → **PASS** se HTTP 400 "Temperatura fora dos limites fisicos (-50 a 80 C)." (Prova que dado nunca chega ao banco.)

## Evidências
Screenshots nítidos das respostas dos 3 cenários pedidos: Login OK, Telemetria Normal, Telemetria Crítica (+ inválido bloqueado como bônus). Recording contínuo da sessão Swagger.
