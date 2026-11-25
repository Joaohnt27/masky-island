> Atualização: a proteção por círculo foi substituída pelo [lampião coletável](../Lantern/README.md). Este relatório registra a etapa anterior.

# Sombra da Tempestade - perseguição durante a exploração

## Comportamento

O NPC explica a ameaça antes de habilitá-la. A sombra persegue na praia, na caverna e no exterior do farol. A casa e o centro são seguros. A aparição introdutória da praia permanece apenas narrativa, antes dessa explicação.

Na primeira saída do ponto seguro, o encontro espera 12 segundos de exploração livre. Um aviso em pergaminho e um sopro anunciam a materialização durante 2,5 segundos. A sombra nasce a pelo menos 3 unidades do jogador e fora do círculo seguro. Ela então persegue por até 9 segundos, com 70% da velocidade do personagem. Como névoa, atravessa obstáculos; o jogador pode escapar movimentando-se, voltando à luz ou saindo da área.

Um círculo dourado identifica o ponto seguro de chegada à cena. Entrar nele afasta a sombra. Se ela alcançar o jogador a 0,5 unidade, ele retorna a esse ponto, com feedback explícito. Inventário, lembranças e puzzles não são apagados. Após o feedback de 3 segundos, há um intervalo de 20 segundos até outra aparição. Permanecer na luz impede a contagem para um novo ataque.

Menus, diálogos, mineração, pausa e transições suspendem movimento e captura da sombra. Ao recuperar os controles, o jogador recebe 1,5 segundo de proteção. Não se acrescentou combate, barra de vida ou perda de itens. No QTE final, a sombra aparece apenas na ilustração e recua a cada acerto.

## Código e configuração

- `StormPursuit.cs`: estados Waiting, Warning, Chasing e Recovering; spawn, perseguição, captura, área segura e feedback.
- `StormShadow.cs`: aparição inicial e dissolução visual no desafio final.
- `GameManager.cs`: registra o aprendizado da ameaça após o NPC e limpa isso em nova partida.
- `DialogueController.cs`: explica lugares perigosos, aviso, fuga e consequência; habilita a mecânica apenas ao concluir a conversa inicial.
- `PlayerClickMovement.cs`: expõe velocidade/limites e oferece retorno ao ponto seguro, cancelando o destino antigo sem apagar bloqueios de outras interfaces.
- `GameAudio.cs`: novo efeito Storm, com o volume e a pausa do canal existente.
- `AuthorStormShadow.cs`: configura praia, caverna, exterior e interior do farol, incluindo sprite, luz segura, papel e DynaPuff. Pode ser executado novamente sem duplicar componentes.

Os parâmetros de velocidade, duração, intervalo e alcance ficam serializados no Inspector. A fonte e o papel são os já utilizados pelo jogo. O sprite foi gerado com image_gen; o prompt está em image-prompt.txt. O som foi sintetizado pelo script CreateStormSfx.py, sem gravações externas.

## Validação

Validação concluída em Play Mode na Unity 6000.6.0f1, em Builds/PV: **40 verificações aprovadas**, processo encerrado com código 0. Resultado completo em storm-pursuit-results.txt. As capturas de aviso, perseguição, captura e QTE foram revisadas visualmente. A primeira tentativa anterior foi bloqueada pela licença; a sessão final conseguiu abrir e executar o Editor.

Os testes exercitam o diálogo inicial, habilitação da ameaça, luz segura, distância de spawn, pausa, bloqueios de interação, captura, manutenção de itens e lembranças, reset e o desafio final com vídeo. Os desafios avançados usam preparação de estado; não representam uma partida completa manual. A perseguição foi exercitada na caverna; o balanceamento nas demais áreas ainda merece avaliação jogando normalmente. O validador aguarda a inicialização da cena antes de verificar o ponto de retorno.

As quatro cenas principais foram atualizadas após aprovação e revisão das capturas, e seus hashes foram comparados com as cenas testadas. Backup anterior em Builds/StormShadowBackup. Não houve publicação no Itch.io nem atualização de build nesta etapa.

## Arquivos aplicados e configuração

Criados: Assets/Scripts/StormShadow.cs; Assets/Scripts/StormPursuit.cs; Assets/Editor/AuthorStormShadow.cs; Assets/Art/Characters/StormShadow/StormShadow-v1.png e SafeLight.mat; Assets/Resources/CartoonSfx/Storm.wav; arquivos meta correspondentes; ferramentas em Tests/AudioAuthoring, Tests/UIAuthoring e Tests/GameplayValidation; capturas e relatório nesta pasta. O prompt completo da arte consta em image-prompt.txt.

Modificados: GameManager.cs, GameAudio.cs, DialogueController.cs, LighthouseInteraction.cs, PlayerClickMovement.cs; cenas Beach, Cave, LighthouseExterior e Lighthouse; README do projeto.

Nenhum campo precisa ser arrastado no Inspector: personagens, área segura, efeito sonoro, fonte e avisos estão configurados. Nenhum asset necessário está faltando. Inicie pelo menu, conclua a conversa inicial com o NPC e explore uma das áreas perigosas fora do círculo de luz. Testar uma cena avançada isolada sem o diálogo não habilita a ameaça.

O Editor registrou sua exceção conhecida de indexação de busca, separada dos erros de gameplay pelo validador. Não houve erro de compilação ou exceção de gameplay na execução aprovada.
