# Inventário por tecla e mineração com descoberta

## Entrega

- Durante a missão, **I** abre/fecha o inventário. **Esc** fecha primeiro o inventário, sem abrir a pausa junto. Também existe um botão de fechar.
- A janela fica fechada por padrão, com um lembrete pequeno de tecla. Encontrar um cristal não abre a janela automaticamente.
- Enquanto o inventário está aberto, o personagem não anda nem inicia interações. Ele não pode ser aberto durante mineração, diálogo ou outra interação que bloqueie movimento. Fechar, desabilitar, reiniciar e trocar de cena liberam o bloqueio corretamente.
- Cada tentativa de mineração dura **4 segundos** e possui **35% de chance** de encontrar um cristal ainda não coletado. Nos demais casos aparece uma mensagem de que nenhum cristal foi encontrado.
- Cancelar não entrega item. A pausa congela a tentativa. Cristais encontrados continuam persistindo entre Island e Cave; os encaixes continuam sendo o desafio final dessa etapa.

A chance vale por tentativa completa, sem limite de tentativas ou recompensa garantida após uma sequência de falhas. Os valores são um ponto inicial de balanceamento.

## Arquivos modificados

- `Assets/Scripts/CrystalInventoryUI.cs`: tecla I, estado aberto/fechado, bloqueio do movimento, botão e lembrete.
- `Assets/Scripts/PauseController.cs`: prioridade de Esc para fechar o inventário.
- `Assets/Scripts/MiningSpot.cs`: duração padrão e chance configurável; tentativa sem recompensa com feedback.
- `Assets/Scripts/MiningUI.cs`: orientações deixam claro que uma tentativa pode não encontrar cristal.
- `Assets/Scripts/DialogueController.cs`: NPC explica o inventário por I e a descoberta não garantida.
- `Assets/Scenes/Island.unity` e `Cave.unity`: janela central, botão e lembrete configurados; Island recebe duração/chance de mineração.
- `Tests/GameplayValidation/WorldMiningValidation.cs`: testes das teclas, bloqueios e resultados de sucesso/falha.

## Arquivos criados

- `Tests/UIAuthoring/AuthorInventoryKeys.cs`: autoria incremental das cenas em cópia isolada. Não executar novamente sobre as cenas prontas.
- Este relatório, resultados e capturas da validação.

## Configuração no Unity

Nenhuma ligação manual adicional. Para balancear, selecione **Island → MiningSpot**:

- **Mining Duration = 4** (segundos).
- **Crystal Chance = 0.35** (35%; 0 nunca encontra, 1 sempre encontra).

Não há moedas ou prazo global de missão. O inventário continua usando os dados do GameManager existente, sem novo gerenciador persistente.

## Assets e limites

Nenhum asset novo de arte é necessário. A picareta mantém o emote simples por código; clips completos do personagem continuam faltando. O inventário permanece específico dos cristais, sem equipamentos ou salvamento em disco.

## Validação

- 49 verificações da suíte de mineração/inventário aprovadas: teclas I/Esc via New Input System, fechamento por botão, bloqueios, pausa, cancelamento, falha sem recompensa, sucesso, persistência e encaixes.
- Conferidos na cena os padrões de quatro segundos e 35%. Nos cenários automatizados de resultado, a chance é forçada para 0% e 100% apenas na cópia de teste, evitando testes aleatórios instáveis; isso não é uma medição estatística da frequência de 35%.
- Capturas inspecionadas em 1280 × 720: KeyboardInventoryPreview.png e MiningUnsuccessfulPreview.png. Outras proporções de tela ainda precisam de conferência manual.
- Compilação sem erros de código. Avisos existentes de APIs obsoletas e uma exceção conhecida do índice de busca do Editor foram registrados separadamente dos testes de gameplay.
- Relatório detalhado: InventoryKeysValidationResults.txt.

- Regressão do primeiro puzzle: 34 verificações aprovadas (FirstPuzzleRegressionAfterInventoryKeys.txt). Total desta etapa: 83 verificações.
