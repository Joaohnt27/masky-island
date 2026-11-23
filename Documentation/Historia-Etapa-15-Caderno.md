# Etapa 15 — Caderno de missões e enigma no altar

## Implementado
Ícone PNG transparente de caderno de viagem no canto inferior direito dos cinco cenários. Clique ou J abre/fecha; Esc fecha antes de pausar. O painel mostra tarefas e anotações em duas páginas. Tarefas concluídas ficam riscadas; próximas tarefas aparecem apenas quando o progresso anterior é alcançado. O conteúdo usa os estados já existentes do GameManager, sem duplicar missão ou salvamento. Nova partida limpa o progresso mostrado.

## Arquivos criados
- Assets/Scripts/AltarPuzzleLayout.cs e .meta: posiciona os encaixes sobre as formas do altar, acompanhando a câmera.
- Tests/UIAuthoring/AuthorAltarPuzzle.cs e Tests/GameplayValidation/AltarPuzzleValidation.cs.
- Assets/Art/UI/Icons/Journal-v1.png e .meta, gerado com image_gen.
- Assets/Scripts/QuestJournalUI.cs e .meta: apresentação e integração ao progresso.
- Tests/UIAuthoring/AuthorQuestJournal.cs.
- Tests/GameplayValidation/QuestJournalValidation.cs.
- Documentation/JournalGenerationPrompt.txt: prompt utilizado.

## Arquivos modificados
- Assets/Scripts/MiningUI.cs: instrução para selecionar o cristal e clicar na mesma forma do altar. Reutiliza a lógica existente de coleta, seleção e resolução.
- Assets/Scripts/PauseController.cs: prioridade de Esc para fechar o caderno.
- Beach, House, Island, Cave e Lighthouse: ícone, painel, referências e bloqueios de foco.

## Configuração e assets
Nenhuma configuração manual necessária. Nenhum asset obrigatório faltante para o caderno. Texto e risco das tarefas usam TextMeshPro. O nome escolhido identifica o caderno sem alterar as falas de amnésia.

## Pendente de identificação
A área vermelha foi identificada pelo usuário como o altar do cenário. Após coletar três cristais, o jogador escolhe um cristal na faixa inferior e clica na forma correspondente sobre a pedra. Uma tentativa incorreta preserva o cristal; completar os três encaixes recupera a lembrança e risca a tarefa no caderno. O elemento laranja ainda precisa de imagem ou descrição para ser removido com precisão.

## Validação
Caderno: 19 verificações aprovadas no Unity 6 em cópia isolada, incluindo tarefas progressivas, riscos, bloqueio de movimento, conflitos com mapa/inventário, J/Esc, cinco cenários, reinício e ausência de transbordamento de texto. Capturas revisadas visualmente. Resultados em Documentation/QuestJournal/ValidationResults.txt. O Editor registrou uma exceção conhecida do índice de busca; ela foi identificada separadamente dos erros de gameplay.

Altar: 12 verificações aprovadas no Unity 6: bloqueio antes dos três cristais, liberação após coleta, raycast dos três encaixes, erro sem perda de cristal, três acertos, lembrança recuperada, fechar/reabrir e integração com as missões. Capturas em 1280×720 revisadas; encaixes alinhados ao cenário e textos sem sobreposição. Resultados em Documentation/QuestJournal/AltarValidationResults.txt. Compilação concluída sem erros. Recomenda-se uma partida manual completa para avaliar conforto e ritmo das interações.


