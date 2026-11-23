# Etapa 05 — o bilhete da volta

## Gameplay

Após concluir os encaixes dos cristais, converse com o estranho. Ele orienta a voltar ao abrigo na cena Cave e reler o bilhete à esquerda.

Aproxime-se do bilhete existente e pressione **E**. A pista conta uma viagem: partir em um barco, seguir um peixe e retornar ao farol. A sequência correta é **BARCO → PEIXE → FAROL**. Os botões aparecem em outra ordem para que o jogador use a pista.

Erros reiniciam somente a sequência atual. Fechar o bilhete limpa uma tentativa parcial. Acertar recupera a terceira lembrança: **“Você voltou para terminar o que começou.”** Depois, o jogador pode voltar ao NPC para ouvir o reconhecimento da descoberta.

Antes da segunda lembrança, o bilhete preserva a orientação inicial para procurar o estranho. Os comandos do terceiro puzzle ficam bloqueados e o feedback explica o pré-requisito. A memória persiste entre cenas, não pode ser concedida duas vezes e é limpa ao reiniciar.

Esta etapa não encerra o jogo automaticamente. Acender o farol e apresentar a partida continuam sendo a próxima entrega.

## Arquivos modificados

- `Assets/Scripts/SymbolSequencePuzzle.cs`: reutilizado nos dois puzzles; novos símbolos adicionados ao fim do enum para preservar referências antigas, pré-requisito opcional e pista que muda com o progresso. Os campos dos três botões existentes foram mantidos para preservar a cena do primeiro puzzle.
- `Assets/Scripts/GameManager.cs`: impede recuperar a memória Return antes das duas anteriores.
- `Assets/Scripts/DialogueController.cs`: orientação ao bilhete após os cristais e falas específicas após a terceira lembrança.
- `Assets/Scenes/Cave.unity`: adaptados ScrollPanel, Scroll_Interactable e InteractionPrompt existentes; adicionados três botões e feedback. Mantidas a posição original da interação e a mensagem de abertura. O painel passa a usar o Canvas de gameplay, atrás da pausa.

## Arquivos criados

- `Tests/UIAuthoring/AuthorThirdPuzzle.cs`: autoria incremental da cena em cópia isolada. Não executar novamente sobre a cena pronta.
- `Tests/GameplayValidation/ThirdPuzzleValidation.cs` e `RunThirdPuzzle.ps1`: validação do terceiro puzzle e captura do cenário/painel.
- Este relatório, resultados e capturas.

## Configuração manual no Unity

Nenhuma ligação adicional: player, painel, prompt, callbacks dos símbolos e fechamento já estão atribuídos. A interação continua usando distância e a tecla E do New Input System. Não foi criado outro sistema de diálogo, interação ou persistência.

## Assets

Nenhum asset externo necessário. O puzzle usa botões com nomes legíveis e o bilhete já existente. Ilustrações próprias de barco, peixe e farol podem ser acrescentadas depois; a mecânica não depende delas.

## Validação

- 37 verificações do terceiro puzzle aprovadas: entrada por E com teclado virtual, orientação inicial preservada, pré-requisito, raycasts dos botões, textos, erros, pausa, fechamento, bloqueio do inventário, solução, recompensa única, diálogo, persistência e reset.
- Regressões: 34 verificações do primeiro puzzle e 49 da mineração/inventário aprovadas. Total: 120 verificações.
- Compilação sem erros de código. Persistem avisos de APIs obsoletas e a ocorrência conhecida do índice de busca do Editor, registrada separadamente de exceções de gameplay.
- Capturas em 1280 × 720 inspecionadas: ThirdPuzzlePreview.png, ThirdMemoryPreview.png e ThirdPuzzleLocation.png. Ajustadas as margens herdadas do texto antigo para manter a pista centralizada.
- Relatórios: ThirdPuzzleValidationResults.txt, FirstPuzzleRegressionAfterThird.txt e WorldMiningRegressionAfterThird.txt.
- Testes executados em cópia isolada da Unity. O teste posiciona o personagem perto do bilhete e aciona E; não substitui uma sessão manual completa de caminhada pela caverna. Outras proporções de tela ainda precisam de conferência manual.

