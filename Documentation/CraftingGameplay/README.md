# Fabricação da chave e mochila ilustrada

## Gameplay
Depois de encaixar os três cristais, recolha os materiais do esconderijo da caverna e volte à bancada da casa. Clique para apoiar o molde, encaixe a peça de metal e acerte três golpes quando o indicador estiver na faixa dourada. Use o botão GOLPEAR ou Espaço. Recolha a chave pronta para guardá-la na mochila.

Erros não retiram materiais nem apagam golpes já acertados. Sair com Esc preserva os materiais; reabrir reinicia a montagem. Os materiais são consumidos pelo GameManager somente ao recolher a chave. A fabricação bloqueia movimento, respeita pausa e impede recompensas duplicadas.

## Mochila
Nova ilustração de couro, tecido verde e bolsos de linho, seguindo a arte do jogo. Seis espaços mostram ícones, nomes e quantidades; clicar em um item atualiza seu ícone ampliado e descrição na etiqueta lateral. Continua acessível por I após coletar a mochila. Texto com quebra de linha e fontes do projeto.

## Arquivos criados
- Assets/Scripts/KeyCraftingUI.cs e meta: etapas de montagem, indicador, golpes e coleta.
- Assets/Art/UI/OpenBackpack-v1.png e meta: fundo ilustrado da mochila.
- Tests/UIAuthoring/AuthorCraftingGameplay.cs: montagem das interfaces no projeto isolado de validação.
- Tests/GameplayValidation/CraftingGameplayValidation.cs: validação em Play Mode.
- Esta pasta: relatório, prompt da arte, resultados e quatro capturas do jogo.

## Arquivos modificados
- Assets/Scripts/ScrollInteraction.cs: abre a bancada interativa em vez de fabricar por diálogo.
- Assets/Scripts/PauseController.cs: Esc fecha a bancada e devolve o controle.
- Assets/Scripts/CrystalInventoryUI.cs: seleção visual e ícone ampliado dos itens.
- Assets/Scenes/Beach.unity, House.unity, Island.unity, Cave.unity e Lighthouse.unity: mochila ilustrada e referências configuradas. House também recebe a interface de fabricação.

## Configuração manual e assets faltantes
Nenhuma configuração no Inspector é necessária. As referências já estão nas cenas. Não faltam assets para esta etapa. Se uma cena estiver aberta com alterações não salvas no Editor, confira essas alterações antes de recarregar a versão atualizada do disco.

## Testes
Unity 6000.6.0f1, projeto isolado Builds/PV: 45 verificações aprovadas e processo encerrado com código 0. Incluem ordem de montagem, tentativas incorretas, três acertos, clique e Espaço, bloqueio de ações repetidas, pausa, cancelamento, consumo de materiais, chave única e inventário nas cinco cenas. Capturas revisadas em 1280×720; descrição corrigida para permanecer na etiqueta. Fontes dos quatro scripts finais conferidas contra as usadas na validação.

As cenas validadas foram transferidas preservando os GUIDs locais do script novo e da arte. Backup das cinco cenas anteriores em Builds/CraftingGameplayBackup. O teste automatizado prepara o progresso necessário; ainda é recomendável uma partida manual desde o menu para avaliar ritmo e dificuldade. O cenário exterior e o cadeado do farol não fazem parte desta etapa.
