# Etapa 10 — Aproximação das três rochas

## Problema encontrado
Os cliques usavam a mesma altura de destino (limite superior antigo do movimento). A distância geral da área cobria quase toda a caverna, permitindo iniciar mineração longe da rocha.

## Alterações
- Cada um dos três colliders MineralVein tem um ponto próprio junto à rocha desenhada.
- Clicar escolhe esse ponto, faz o personagem caminhar e inicia a mineração somente ao chegar.
- O limite superior do movimento WASD na caverna permite chegar ao chão junto às rochas.
- Iniciar mineração exige proximidade de até 0,25 unidade do ponto escolhido. Sair dessa proximidade durante uma tentativa cancela a extração.
- O alvo não pode mudar durante uma mineração ativa.
- O altar dos encaixes tem uma posição separada; mantém o puzzle existente.
- Permanecem os quatro segundos de mineração, as probabilidades, o inventário e os sons.

## Arquivos modificados
- Assets/Scripts/MiningSpot.cs: referências das três rochas/pontos, escolha do alvo e validação de proximidade.
- Assets/Scripts/PlayerClickMovement.cs: usa a posição da rocha clicada, sem forçar uma altura comum.
- Assets/Scenes/Cave.unity: pontos de aproximação e limite de movimento configurados.

## Arquivos criados
- Tests/UIAuthoring/AuthorRockApproaches.cs: configuração incremental da caverna na cópia isolada.
- Tests/GameplayValidation/RockApproachValidation.cs: cliques reais nas três rochas, deslocamento, bloqueios, coleta e altar.

## Configuração manual no Unity
Nenhuma. As referências dos três colliders e respectivos pontos já ficam gravadas na cena Cave. O estado de mineração continua no componente existente, sem três sistemas de mineração paralelos.

## Assets faltantes
Nenhum asset obrigatório novo. A picareta/emote existente acompanha o personagem; não foi acrescentado um novo sprite de animação.

## Testes
- Compilação Unity 6 concluída sem erros.
- 27 verificações aprovadas (RockApproaches/ValidationResults.txt).
- Cliques simulados pelo New Input System nas três rochas: aproximação individual, mineração só após chegada, bloqueio de movimento/retarget e coleta no inventário.
- Altar acessível após três cristais e WASD alcançando a faixa superior do chão.
- Capturas das três posições revisadas visualmente em 1280×720 (RockApproaches/rock-1-mining.png, rock-2-mining.png e rock-3-mining.png).
- O teste força sucesso nas tentativas somente em runtime da cópia isolada; a chance de produção continua 35% e a duração continua 4 segundos.
- A exceção conhecida do índice de busca do Editor foi registrada separadamente; nenhuma exceção de gameplay aceita.
- Validação focada na caverna; o percurso completo até os créditos não foi repetido nesta etapa.
