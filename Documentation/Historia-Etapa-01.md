# História — etapa 01: análise e roteiro

Data: 05/11/2026. Etapa concluída: análise e definição do primeiro recorte.
Este documento descreve o plano; os puzzles abaixo ainda não estão implementados.

Atualização após esta análise: o usuário decidiu retirar moedas e timer inteiramente. As referências abaixo ao modo monetário e ao prazo são histórico do diagnóstico, não requisitos atuais. Consulte Historia-Etapa-02.md para a implementação dessa retirada.

## Projeto encontrado

- Unity 6000.6.0f1. Cenas habilitadas: Splash, MainMenu, Cave, Island e Credits.
- O menu inicia Cave, não a praia. Cave leva a Island; Island tem retorno para Cave e uma segunda porta com destino vazio, atualmente ignorada pelo movimento.
- PlayerClickMovement usa New Input System, raycast 3D e o chão WalkableArea. Já possui bloqueios por proprietário, reutilizáveis para puzzles.
- NPCInteraction usa distância e tecla E. Em Island, referencia Player, prompt e DialogueController. O diálogo referencia painel, dois textos TMP e movimento do jogador. A última fala chama StartMission.
- Interactable e ScrollInteraction já permitem ler pergaminhos e bloquear o movimento durante a leitura.
- GameManager e MoneyManager persistem entre cenas e descartam duplicatas. SettingsManager também persiste; SceneFade cria um objeto persistente para a transição.
- GameManager controla missão, pausa, prazo de 1200 segundos, vitória e derrota. Hoje completar a missão exige pelo menos 250 moedas, inclusive dentro de CompleteMission.
- MoneyManager notifica mudanças por evento. MoneyUI e TimerUI ocultam os textos antes da missão; há componentes de HUD e resultado em Cave e Island.
- Island tem Canvas de gameplay, diálogo, pergaminho, mineração e Canvas de menus. MiningUI referencia o ponto, painel, botão, Slider e texto de resultado. Os nomes MinigPanel/MinigButton existentes serão preservados.
- MiningSpot usa distância 3D (não trigger), coroutine de 3 segundos, bloqueio de movimento, recompensas configuráveis e falha inicial de 0%. Está em Island, não em Cave.
- MissionResultUI ainda apresenta a história antiga de obter dinheiro para manter a casa.
- Existem rotinas de validação em Tests/GameplayValidation e ferramentas de autoria em Tests/UIAuthoring. Resultados antigos não representam uma execução nesta etapa.

## O que falta para a aventura

Não foram encontrados scripts de puzzles, inventário de lembranças ou progressão do farol nos scripts próprios do jogo. Os diálogos atuais estão fixos no código e contam outra história. A condição de vitória monetária precisa ser adaptada antes de ativar a campanha de puzzles.

Existem imagens de jogador, NPC, diálogo, pergaminho, ilha e caverna em Assets/Art/Backgrounds. Não foram encontrados arquivos .anim, .controller ou prefabs próprios de gameplay na busca em Assets; os prefabs encontrados pertencem aos exemplos TMP. A existência de player_sprites.png não confirma uma animação de mineração. Os eventos opcionais de animação do MiningSpot estão sem listeners na cena.

## Roteiro adaptado ao cenário existente

Título de trabalho: **Mask Island — A Ilha dos Esquecidos**.

O personagem acorda em um abrigo na caverna sem lembrar como chegou. Um bilhete o orienta a procurar o estranho de capuz na ilha. O estranho revela que o farol apagado impede a partida e que três lembranças podem restaurar sua luz. Ele assume o papel narrativo do barqueiro, aproveitando o NPC atual.

Falas propostas para o encontro inicial:

1. "Você acordou no abrigo? Então a ilha trouxe mais alguém de volta."
2. "Aqui, as lembranças se perdem. Mas deixam pistas."
3. "O farol está apagado. Sem sua luz, nenhum barco encontra a saída."
4. "Três lembranças podem acendê-lo outra vez. A primeira está escondida entre os símbolos da ilha."
5. "Procure a inscrição. Ela conta a ordem em que o céu muda."

Pista proposta: "O sol se despede. A lua desperta. A estrela mostra o caminho."

Esses textos serão ativados junto com o primeiro puzzle, para não orientar o jogador a uma interação inexistente.

## Progressão planejada

| Passo | Interação | Resultado |
| --- | --- | --- |
| Abertura | Sair de Cave e conversar com o NPC em Island | Inicia a aventura; indica a inscrição |
| Puzzle 1 | Ler a pista e pressionar Sol → Lua → Estrela | Primeira lembrança: "Você cuidava da luz." |
| Puzzle 2 | Minerar três pontos determinados e encaixar cristais por símbolo | Segunda lembrança: "Na noite da tempestade, o farol se apagou." |
| Puzzle 3 | Ler bilhete e ordenar Barco → Peixe → Farol | Terceira lembrança: "Você voltou para terminar o que começou." |
| Final | Usar a máscara no mecanismo com as três lembranças | Acende o farol e apresenta a partida |

No primeiro protótipo, os puzzles podem abrir painéis a partir de pontos de interação em Island/Cave. Praça, casa e farol não precisam começar como novas cenas. Cristais obrigatórios serão garantidos, sem depender do sorteio de moedas. Erros reiniciam a tentativa sem remover lembranças; puzzles resolvidos não concedem recompensa novamente.

## Decisões para a implementação incremental

- Estender GameManager como fonte única da progressão, guardando apenas dados persistentes, sem referências a painéis de cena.
- Retirar o sistema de moedas; a campanha terá vitória pelo objetivo final da história.
- Retirar o timer de missão e a derrota por tempo; exploração sem limite.
- Reutilizar os bloqueios de movimento, TMP, painéis e interação existentes. Sem novos packages.
- Não implementar loja de dicas nesta primeira versão; dicas podem vir do NPC sem custo.

## Próximas entregas pequenas

1. **Etapa 02 — primeira parte jogável:** modo de história e progresso no GameManager; abertura revisada; puzzle de três símbolos com pista, erro, acerto e primeira lembrança; integração na cena e validação. Arquivos previstos: GameManager.cs, DialogueController.cs, Island.unity e um componente novo para o puzzle. Adaptar MoneyUI/MissionResultUI somente onde necessário ao novo modo. Detalhar referências finais no relatório da implementação.
2. **Cristais:** adaptar mineração para recompensas de história garantidas e implementar encaixes, sem moedas.
3. **Etapa 04 — farol:** terceiro puzzle, três lembranças exigidas e final simples.
4. **Etapa 05 — revisão:** dicas, leitura dos textos, reinício, pausa, transições e teste completo do começo ao fim.

## Relatório desta etapa

- Arquivo criado: Documentation/Historia-Etapa-01.md.
- Arquivos existentes modificados: nenhum.
- Mecânicas novas implementadas: nenhuma nesta etapa de análise e roteiro.
- Configuração manual necessária no Unity: nenhuma nesta etapa.
- Assets pendentes para o plano: símbolos dos puzzles, cristais e apresentação visual do farol/final. Podem começar como botões TMP e formas simples. Animação de mineração continua sem clips/controller identificados.
- Validação realizada: leitura dos scripts centrais, inventário de cenas/assets, inspeção das referências serializadas de NPC, diálogo e mineração e das rotas entre cenas.
- Não foi executado Play Mode nem compilação nesta etapa documental. Layout, alcance das interações e funcionamento em execução ainda precisam ser testados nas etapas de implementação.
