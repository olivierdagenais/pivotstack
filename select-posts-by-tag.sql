SELECT
    p.Id AS QuestionId,
    p.Title,
    p.Body AS QuestionBody,
    p.Score,
    p.ViewCount AS Views,
    p.AnswerCount AS Answers,
    p.Tags,
    p.CreationDate AS DateAsked,
    (
        SELECT
            MIN(CreationDate) AS Expr1
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
    ) AS DateFirstAnswer,
    (
        SELECT
            MAX(CreationDate) AS Expr1
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
    ) AS DateLastAnswer,
    COALESCE (
        p.OwnerDisplayName,
        (
            SELECT
                DisplayName
            FROM
                Users AS u1
            WHERE
                (Id = p.OwnerUserId)
        ),
        (
            SELECT
                DisplayName
            FROM
                Users AS u2
            WHERE
                (Id = p.LastEditorUserId)
        )
    ) AS Asker,
    p.AcceptedAnswerId,
    (
        SELECT
            Body
        FROM
            Posts AS p2
        WHERE
            (p.AcceptedAnswerId = Id) AND (PostTypeId = 2)
    ) AS AcceptedAnswerBody,
    (
        SELECT
            TOP (1) Id
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
        ORDER BY
            Score DESC
    ) AS TopAnswerId,
    (
        SELECT
            TOP (1) Body
        FROM
            Posts AS p2
        WHERE
            (p.Id = ParentId)
        ORDER BY
            Score DESC
    ) AS TopAnswerBody,
    COALESCE (p.FavoriteCount, 0) AS Favorites
FROM
    Posts AS p
INNER JOIN
    PostsTags AS pt ON p.Id = pt.PostId
WHERE
    (p.PostTypeId = 1) AND (pt.Tag = @tag)
