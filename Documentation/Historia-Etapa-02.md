# Etapa 02 — retirada de moedas e timer

Data: 05/11/2026.

## Escopo

Conforme a nova orientação, a aventura não possui moedas, meta financeira, contagem regressiva ou derrota por tempo. Essa decisão substitui os requisitos anteriores desses sistemas. Não será mantido um modo monetário separado.

O NPC continua iniciando a missão ao concluir o diálogo. A exploração pode continuar sem limite. O objetivo final dos puzzles ainda não foi implementado: CompleteMission permanece disponível para ser chamado por essa interação futura, sem uma vitória automática nesta etapa.

## Arquivos removidos

- Assets/Scripts/MoneyManager.cs e seu .meta.
- Assets/Scripts/MoneyUI.cs e seu .meta.
- Assets/Scripts/TimerUI.cs e seu .meta.

## Arquivos modificados

- Assets/Scripts/GameManager.cs: retirados saldo/meta, prazo e estado de derrota. Mantidos início único, conclusão explícita, pausa, persistência, reset e navegação.
- Assets/Scripts/PauseController.cs: retirada a dependência do estado de derrota por tempo.
- Assets/Scripts/MissionResultUI.cs: resultado sem dinheiro ou prazo; mensagem de conclusão da jornada.
- Assets/Scripts/DialogueController.cs: substituídas as dez falas financeiras por cinco falas introdutórias sobre a ilha, as lembranças e o farol. Não promete um puzzle já disponível.
- Assets/Scripts/MiningSpot.cs: retirado o sorteio de moedas e a dependência de prazo/saldo. Mantidos os três segundos de escavação, cancelamento, progresso, pausa e bloqueio de movimento. Evento onMiningCompleted preparado para a futura coleta de cristais; ainda não concede itens.
- Assets/Scenes/Cave.unity e Island.unity: removidos os objetos de dinheiro e timer, seus componentes e referências de hierarquia. Atualizados os pergaminhos. Em Island, MiningSpot, MiningUI, MinigPanel e MiningAreaHint ficam inativos até a integração dos cristais.
- Tests/GameplayValidation/GameplayValidationRunner.cs: substituídos testes financeiros e de timeout por verificações da missão sem prazo, ausência dos objetos antigos, diálogo, persistência, pausa, escavação sem recompensa monetária e conclusão explícita.
- Tests/GameplayValidation/Run.ps1: cada execução usa uma cópia nova para não reaproveitar scripts removidos em cópias antigas.
- Documentation/Historia-Etapa-01.md: atualizado o plano conforme a decisão de retirar moedas e timer.
- Documentation/Gameplay.md e PauseAndEndGame.md: marcados como registros históricos da versão anterior.

## Arquivos criados

- Este relatório. Scripts auxiliares de migração/checagem e saída de compilação ficam em Temp/StoryMigration; não são assets do jogo.

## Configuração manual necessária

Nenhuma para esta retirada. As alterações de referências já foram feitas nas cenas. A mineração foi desativada intencionalmente; não é necessário reativá-la no Inspector agora.

## Assets faltantes e próximo passo

Esta etapa não exige arte nova. Continuam pendentes os símbolos, cristais, apresentação do farol/final e animação de mineração. Próxima entrega: o primeiro puzzle Sol → Lua → Estrela e sua lembrança, reutilizando o GameManager.

## Validação

- Compilação C# com Roslyn e referências locais da Unity: aprovada, sem erros, incluindo scripts de gameplay e runner de testes. Há avisos de APIs de busca obsoletas e de campos serializados preenchidos pelo Inspector.
- Verificação estática de Cave e Island: sem IDs duplicados, sem referências internas pendentes, sem referências a scripts próprios removidos e sem objetos de dinheiro/timer.
- Play Mode não concluído: a primeira execução falhou na comunicação local de licença/Burst. A segunda reconheceu a licença, mas permaneceu na importação inicial de assets sem produzir relatório de testes. Apenas a instância da cópia de validação foi encerrada; o Editor do projeto principal foi preservado. Não considerar os testes de execução aprovados. Log: Temp/StoryMigration/unity-validation.log.
- Inspeção visual no Editor ainda necessária para conferir a leitura dos textos no layout existente.
- Teste manual pendente: iniciar pelo menu, sair de Cave para Island, concluir as cinco falas do NPC, confirmar ausência de moedas/timer, abrir/fechar pausa, retornar a Cave e ao menu. O painel de mineração deve permanecer oculto. A conclusão final não tem interação de jogador nesta etapa.
