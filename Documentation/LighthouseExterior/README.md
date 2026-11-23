# Exterior do farol e cadeado

## Fluxo implementado
Centro ou mapa → exterior do farol → inscrição → usar a chave fabricada → resolver a combinação → entrar → QTE existente → cena de encerramento com o vídeo final.

A placa à esquerda da porta e a própria porta são clicáveis. O personagem caminha até o ponto de interação. O painel usa o pergaminho e a fonte DynaPuff do projeto, com uma nova arte de cadeado e três seletores numéricos (+ / -). Esc fecha e devolve o movimento. A chave abre a fechadura; a combinação solta o cadeado. Tentativas incorretas não consomem itens.

A inscrição apresenta quadrado, círculo e triângulo, nessa ordem. Cada algarismo conta os lados retos do sinal: **403**. O círculo corresponde a zero. Ler a inscrição registra a pista no diário. É necessário fabricar e usar a chave, além de ler a pista, para destrancar. A chave permanece na mochila como item da história.

O progresso pertence ao GameManager existente: leitura, uso da chave e desbloqueio persistem nas trocas de cena, mas são limpos em uma nova partida. O cadeado sobre a porta desaparece após a solução. O mapa mantém um único ponto chamado Farol, agora destinado ao exterior. Rotas antigas para o interior são redirecionadas enquanto a porta estiver trancada. O QTE e a vitória também verificam o desbloqueio.

## Arquivos criados
- Assets/Scenes/LighthouseExterior.unity e meta.
- Assets/Scripts/LighthouseGate.cs e meta: painel, combinação e entrada.
- Assets/Scripts/LighthouseInscription.cs e meta: interação com a placa.
- Assets/Art/Backgrounds/Story/06-farol-exterior.png e meta.
- Assets/Art/UI/LighthousePadlock-v1.png e meta.
- Tests/UIAuthoring/AuthorLighthouseExterior.cs: montagem na cópia de validação.
- Tests/GameplayValidation/LighthouseExteriorValidation.cs: teste do fluxo e das restrições.
- Esta pasta: capturas, resultados, relatório e prompts das duas artes geradas pela ferramenta integrada image_gen.

## Arquivos modificados
- GameManager.cs: estado persistente da porta, reset, reconhecimento da área externa e condição de vitória.
- SceneFade.cs: proteção da entrada antiga para o interior.
- IslandMapGraphic.cs e IslandMapUI.cs: destino exterior e compatibilidade com os botões existentes.
- StoryLocation.cs e QuestJournalUI.cs: objetivo de destrancar e anotação da pista.
- LighthouseInteraction.cs: QTE disponível após desbloquear.
- PauseController.cs: fechar o painel do cadeado com Esc.
- Beach.unity, House.unity, Island.unity, Cave.unity e Lighthouse.unity: prévia do mapa; caminho do centro para o exterior; saída do interior para o exterior.
- ProjectSettings/EditorBuildSettings.asset: nova cena incluída no build.

## Configuração no Unity e assets faltantes
Nenhuma configuração manual no Inspector é necessária. Cena, referências, colisores, pontos de aproximação e build estão configurados. Nenhum asset necessário está pendente. A porta usa a transição entre cenas existente; esta etapa não acrescenta uma animação quadro a quadro de abertura da porta.

## Validação
Unity 6000.6.0f1, Play Mode em Builds/PV: **37 verificações aprovadas**, encerramento com código 0. Foram exercitados clique real na placa e na porta, caminhada automática até a entrada, bloqueio de movimento, Esc, pausa, chave ausente, senha incorreta, seleção numérica circular, senha correta, diário, viagem rápida, persistência, reset, QTE e chegada à cena Ending. O teste prepara o progresso anterior e fabrica a chave pelo método já validado na etapa de crafting.

Capturas em 1280×720 revisadas para enquadramento, legibilidade e posição dos controles. Fontes dos dez scripts e das seis cenas finais conferidas com os arquivos testados. Houve uma exceção conhecida do índice de busca do Editor, registrada no resultado; não houve falha de compilação ou exceção de gameplay. O teste confirmou a chegada ao encerramento, sem reproduzir integralmente o vídeo até os créditos.

Backup das cinco cenas anteriores e da lista de build: Builds/LighthouseExteriorBackup. Para avaliar a experiência completa, recomenda-se uma partida manual desde o menu, incluindo o crafting e a leitura espontânea da pista.
